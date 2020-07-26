# To run this script from Visual Studio Code terminal use 'pwsh' and run:
#   cd ~/dev/projects/aspnetcore-logging/Build/db4it-compose
#   sudo pwsh ./RunComposeServices.ps1
Param(
    [String]
    $RelativePathToComposeFile = 'docker-compose.yml',

    [String]
    $RelativePathToEnvironmentFile = '.env',

    [hashtable]
    $ExtraEnvironmentVariables,

    [String]
    $ComposeProjectName = 'aspnet-core-logging-it'
)

$ErrorActionPreference = 'Stop'

Write-Output "Current script path: $PSScriptRoot"
$ComposeFilePath = Join-Path -Path $PSScriptRoot $RelativePathToComposeFile

if (![System.IO.File]::Exists($ComposeFilePath)) {
    Write-Output "There is no compose file at path: $ComposeFilePath"
    exit 1;
}

$ComposeEnvironmentFilePath = Join-Path -Path $PSScriptRoot $RelativePathToEnvironmentFile

if (![System.IO.File]::Exists($ComposeEnvironmentFilePath)) {
    Write-Output "There is no environment file at path: $ComposeEnvironmentFilePath"
    exit 2;
}

$EnvironmentFileLines = Get-Content -Path $ComposeEnvironmentFilePath

foreach ($EnvironmentFileLine in $EnvironmentFileLines) {
    # Each line of text will be split using first delimiter only
    $Parts = $EnvironmentFileLine.Split('=', 2)
    $EnvironmentVariableName = $Parts[0]
    $EnvironmentVariableValue = $Parts[1]

    # Each key-value pair from the environment file will be promoted to an environment variable
    # in the scope of the current process since, AFAIK, there's no other way of passing such variables
    # to the containers started by Docker Compose
    [System.Environment]::SetEnvironmentVariable($EnvironmentVariableName, $EnvironmentVariableValue, 'Process')
}

if ($ExtraEnvironmentVariables -ne $null) {
    $ExtraEnvironmentVariables.GetEnumerator() | ForEach-Object { 
        [System.Environment]::SetEnvironmentVariable($_.Key, $_.Value, 'Process')
    }
}

$ComposeStartInfoMessage = "About to start compose services declared in file: `"$ComposeFilePath`" " `
    + "using project name: `"$ComposeProjectName`" " `
    + "and environment file: `"$ComposeEnvironmentFilePath`" ..."
Write-Output $ComposeStartInfoMessage

docker-compose --file="$ComposeFilePath" `
    --project-name="$ComposeProjectName" `
    up -d 

$LsCommandOutput = (docker container ls -a `
        --filter "label=com.docker.compose.project=$ComposeProjectName" `
        --format "{{ .ID }}") | Out-String

$ComposeServices = [System.Collections.Generic.List[psobject]]::new()
$LsCommandOutput.Split([System.Environment]::NewLine, [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object {
    $ContainerId = $_
    $ComposeServiceLabelsAsJson = (docker inspect --format '{{ json .Config.Labels }}' $ContainerId) | Out-String | ConvertFrom-Json
    $ComposeServiceNameLabel = 'com.docker.compose.service'
    $ComposeServiceName = $ComposeServiceLabelsAsJson.$ComposeServiceNameLabel
    
    $ComposeService = New-Object PSObject -Property @{
        ContainerId = $ContainerId
        ServiceName = $ComposeServiceName
    }

    $ComposeServices.Add($ComposeService)
}

$NumberOfTries = 0
$MaxNumberOfTries = 30
$SleepingTimeInMillis = 250

do {
    Start-Sleep -Milliseconds $SleepingTimeInMillis
    $AreAllServicesReady = $true

    foreach ($ComposeService in $ComposeServices) {
        $IsServiceReady = docker inspect $ComposeService.ContainerId --format "{{.State.Health.Status}}" | Select-String -Pattern 'healthy' -SimpleMatch -Quiet

        if ($IsServiceReady -eq $false) {
            Write-Output "Service: $($ComposeService.ServiceName) from project: $ComposeProjectName is not healthy yet"
            $AreAllServicesReady = $false
            continue;
        }
        else {
            Write-Output "Service: $($ComposeService.ServiceName) from project: $ComposeProjectName is healthy"
        }
    }

    if ($AreAllServicesReady) {
        Write-Output "All services from project: $ComposeProjectName are healthy"

        foreach ($ComposeService in $ComposeServices) {
            $PortCommandOutput = (docker port $ComposeService.ContainerId) | Out-String
            $RawPortMappings = $PortCommandOutput.Split([System.Environment]::NewLine, [System.StringSplitOptions]::RemoveEmptyEntries)

            foreach ($RawPortMapping in $RawPortMappings) {
                # Port mapping looks like this: 5432/tcp -> 0.0.0.0:32769
                # The part on the left side of the ' -> ' string represents container port info,
                # while the part of the right represents host port info.
                #
                # To find the container port, one need to extract it from string '5432/tcp' - in this case, the container port is: 5432.
                # To find the host port, one need to extract it from string '0.0.0.0:32769' - in this case, the host port is: 32769.
                $Parts = $RawPortMapping.Split(' -> ')
                $RawContainerPort = $Parts[0]
                $RawHostPort = $Parts[1]
                $ContainerPort = $RawContainerPort.Split('/')[0]
                $HostPort = $RawHostPort.Split(':')[1]

                # For each port mapping an Azure DevOps pipeline variable will be created with a name following the convention:
                #   compose.project.<COMPOSE_PROJECT_NAME>.service.<COMPOSE_SERVICE_NAME>.port.<CONTAINER_PORT>.
                # The variable value will be set to the host port.
                # Using the port mapping from above and assuming the project name is 'aspnet-core-logging-it' and 
                # the service is named 'db-v12', the following variable will be created:
                #   'compose.project.aspnet-core-logging-it.services.db-v12.ports.5432' with value: '32769'
                Write-Output "##vso[task.setvariable variable=compose.project.$ComposeProjectName.service.$($ComposeService.ServiceName).port.$ContainerPort]$HostPort"
            }
        }

        exit 0;
    }

    $numberOfTries++
} until ($NumberOfTries -eq $MaxNumberOfTries)

exit 99;