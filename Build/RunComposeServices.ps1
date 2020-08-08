# This script starts the Docker Compose services residing in a given compose file and will also
# create one Azure DevOps variable per port per service.
Param(
    # Relative path pointing to a Docker Compose YAML file.
    # This path is resolved using this script location as base path.
    # See more here: https://docs.docker.com/compose/reference/overview/#use--f-to-specify-name-and-path-of-one-or-more-compose-files.
    # Currently, this script supports only one compose file.
    [String]
    $RelativePathToComposeFile = 'docker-compose.yml',

    # Docker Compose project name.
    # See more here: https://docs.docker.com/compose/reference/overview/#use--p-to-specify-a-project-name.
    [String]
    $ComposeProjectName = 'aspnet-core-logging-it',

    # Relative path pointing to a file containing variables following `key=value` convention.
    # This path is resolved using this script location as base path.
    # These variables will be passed to the containers started via Docker Compose.
    [String]
    $RelativePathToEnvironmentFile = '.env',

    # The amount of time in milliseconds between two consecutive checks made to ensure  compose services have 
    # reached healthy state.
    [Int32]
    [ValidateRange(250, [Int32]::MaxValue)]
    $HealthCheckIntervalInMilliseconds = 250,

    # The maximum amount of retries before giving up and considering that the compose services are not running.
    [Int32]
    [ValidateRange(1, [Int32]::MaxValue)]
    $MaxNumberOfTries = 120,

    # An optional dictionary storing variables which will be passed to the containers started via Docker Compose.
    [hashtable]
    $ExtraEnvironmentVariables,

    # The path to the file where Docker Compose will write its logs
    [String]
    $ComposeLogsOutputFilePath
)

Write-Output "Current script path: $PSScriptRoot"
$ComposeFilePath = Join-Path -Path $PSScriptRoot $RelativePathToComposeFile

if (![System.IO.File]::Exists($ComposeFilePath))
{
    Write-Output "There is no compose file at path: `"$ComposeFilePath`""
    exit 1;
}

$ComposeEnvironmentFilePath = Join-Path -Path $PSScriptRoot $RelativePathToEnvironmentFile

if (![System.IO.File]::Exists($ComposeEnvironmentFilePath))
{
    Write-Output "There is no environment file at path: `"$ComposeEnvironmentFilePath`""
    exit 2;
}

$EnvironmentFileLines = Get-Content -Path $ComposeEnvironmentFilePath

foreach ($EnvironmentFileLine in $EnvironmentFileLines)
{
    if($EnvironmentFileLine.Trim().Length -eq 0)
    {
        # Ignore empty lines
        continue;
    }
    
    if($EnvironmentFileLine.StartsWith('#'))
    {
        # Ignore lines representing comments
        continue;
    }
    
    # Each line of text will be split using first delimiter only
    $EnvironmentFileLineParts = $EnvironmentFileLine.Split('=', 2)
    $EnvironmentVariableName = $EnvironmentFileLineParts[0]
    $EnvironmentVariableValue = $EnvironmentFileLineParts[1]

    # Each key-value pair from the environment file will be promoted to an environment variable
    # in the scope of the current process since, AFAIK, there's no other way of passing such variables
    # to the containers started by Docker Compose
    [System.Environment]::SetEnvironmentVariable($EnvironmentVariableName, $EnvironmentVariableValue, 'Process')
}

if ($ExtraEnvironmentVariables -ne $null)
{
    $ExtraEnvironmentVariables.GetEnumerator() | ForEach-Object {
        [System.Environment]::SetEnvironmentVariable($_.Key, $_.Value, 'Process')
    }
}

$CreateVolmeInfoMessage = "About to create Docker volume needed by compose services declared in file: `"$ComposeFilePath`" " `
                         + "using project name: `"$ComposeProjectName`" " `
                         + "and environment file: `"$ComposeEnvironmentFilePath`" ..."
Write-Output $CreateVolmeInfoMessage

# satrapu 2020-07-08 See whether manually creating the external volumes 
# works on Windows-based Azure DevOps agents
docker volume create "db4it_data"

if ($LASTEXITCODE -ne 0)
{
    Write-Output "##vso[task.LogIssue type=error;]Failed to create external Docker volume for project: $ComposeProjectName"
    Write-Output "##vso[task.complete result=Failed;]"
    exit 3;
}

$ComposeStartInfoMessage = "About to start compose services declared in file: `"$ComposeFilePath`" " `
                         + "using project name: `"$ComposeProjectName`" " `
                         + "and environment file: `"$ComposeEnvironmentFilePath`" ..."
Write-Output $ComposeStartInfoMessage
docker-compose --file="$ComposeFilePath" `
               --project-name="$ComposeProjectName" `
               up -d

if ($LASTEXITCODE -ne 0)
{
    Write-Output "##vso[artifact.upload]$LogsCommandOutputFileName"
    Write-Output "##vso[task.LogIssue type=error;]Failed to start compose services for project: $ComposeProjectName"
    Write-Output "##vso[task.complete result=Failed;]"
    exit 4;
}

$LsCommandOutput = docker container ls -a `
                                    --filter "label=com.docker.compose.project=$ComposeProjectName" `
                                    --format "{{ .ID }}" `
                                    | Out-String

if ($LASTEXITCODE -ne 0)
{
    Write-Output "##vso[task.LogIssue type=error;]Failed to identify compose services for project: $ComposeProjectName"
    Write-Output "##vso[task.complete result=Failed;]"
    exit 5;
}

Write-Output "Found the following container IDs: $LsCommandOutput"

$ComposeServices = [System.Collections.Generic.List[psobject]]::new()
$LsCommandOutput.Split([System.Environment]::NewLine, [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object {
    $ContainerId = $_
    $ComposeServiceLabelsAsJson = docker inspect --format '{{ json .Config.Labels }}' $ContainerId `
                                                 | Out-String `
                                                 | ConvertFrom-Json

    if ($LASTEXITCODE -ne 0)
    {
        Write-Output "##vso[task.LogIssue type=error;]Failed to inspect container with ID: $ContainerId"
        Write-Output "##vso[task.complete result=Failed;]"
        exit 6;
    }

    $ComposeServiceNameLabel = 'com.docker.compose.service'
    $ComposeServiceName = $ComposeServiceLabelsAsJson.$ComposeServiceNameLabel

    $ComposeService = New-Object PSObject -Property @{
        ContainerId = $ContainerId
        ServiceName = $ComposeServiceName
    }
    
    $ComposeServices.Add($ComposeService)
    
    $ComposeServiceInfoMessage = "Found compose service with container id: `"$($ComposeService.ContainerId)`" " `
                               + "and service name: `"$($ComposeService.ServiceName)`""
    Write-Output $ComposeServiceInfoMessage
}

if ($ComposeServices.Count -eq 1)
{
    Write-Output "About to check whether the compose service is healthy or not ..."
}
else
{
    Write-Output "About to check whether $($ComposeServices.Count) compose services are healthy or not ..."
}

$NumberOfTries = 1

do
{
    Start-Sleep -Milliseconds $HealthCheckIntervalInMilliseconds
    $AreAllServicesReady = $true

    foreach ($ComposeService in $ComposeServices)
    {
            $IsServiceHealthy = docker inspect "$($ComposeService.ContainerId)" `
                                           --format "{{.State.Health.Status}}" `
                                           | Select-String -Pattern 'healthy' -SimpleMatch -Quiet

        if ($LASTEXITCODE -ne 0)
        {
            Write-Output "##vso[task.LogIssue type=error;]Failed to fetch health state for compose service " `
                       + "with name: $($ComposeService.ServiceName)"
            Write-Output "##vso[task.complete result=Failed;]"
            exit 7;
        }

        if ($IsServiceHealthy -eq $false)
        {
            Write-Output "Service: $($ComposeService.ServiceName) from project: $ComposeProjectName is not healthy yet"
            $AreAllServicesReady = $false
            continue;
        }
        else
        {
            Write-Output "Service: $($ComposeService.ServiceName) from project: $ComposeProjectName is healthy"
        }
    }

    if ($AreAllServicesReady -eq $true)
    {
        break;
    }

    $NumberOfTries++
} until ($NumberOfTries -eq $MaxNumberOfTries)

if ($ComposeServices.Count -eq 1)
{
    Write-Output "Finished checking whether compose service is healthy or not"
}
else
{
    Write-Output "Finished checking whether compose services are healthy or not"
}

if ($AreAllServicesReady -eq $false)
{
    $ErrorMessage = "Not all services from project: $ComposeProjectName " `
                  + "are still not running after checking for $NumberOfTries times; will stop here"
    Write-Output "##vso[task.LogIssue type=error;]$ErrorMessage"
    Write-Output "##vso[task.complete result=Failed;]"
    exit 8;
}

foreach ($ComposeService in $ComposeServices)
{
    Write-Output "About to fetch port mappings for compose service with name: $($ComposeService.ServiceName) ..."
    $PortCommandOutput = docker port "$($ComposeService.ContainerId)" | Out-String

    if ($LASTEXITCODE -ne 0)
    {
        Write-Output "##vso[task.LogIssue type=error;]Failed to fetch port mappings for compose service " `
                     "with name: $($ComposeService.ServiceName)"
        Write-Output "##vso[task.complete result=Failed;]"
        exit 9;
    }
    
    if($PortCommandOutput.Length -eq 0)
    {
        Write-Output "This compose service has no port mappings"
        break;
    }

    Write-Output "Found port mappings: $PortCommandOutput"
    $RawPortMappings = $PortCommandOutput.Split([System.Environment]::NewLine, [System.StringSplitOptions]::RemoveEmptyEntries)

    foreach ($RawPortMapping in $RawPortMappings)
    {
        Write-Output "Processing port mapping: `"$RawPortMapping`" ..."

        # Port mapping looks like this: 5432/tcp -> 0.0.0.0:32769
        # The part on the left side of the ' -> ' string represents container port info,
        # while the part of the right represents host port info.
        #
        # To find the container port, one need to extract it from string '5432/tcp' - in this case, 
        # the container port is: 5432.
        # To find the host port, one need to extract it from string '0.0.0.0:32769' - in this case, 
        # the host port is: 32769.
        $RawPortMappingParts = $RawPortMapping.Split(' -> ', [System.StringSplitOptions]::RemoveEmptyEntries)
        $RawContainerPort = $RawPortMappingParts[0]
        $RawHostPort = $RawPortMappingParts[1]
        $ContainerPort = $RawContainerPort.Split('/', [System.StringSplitOptions]::RemoveEmptyEntries)[0]
        $HostPort = $RawHostPort.Split(':', [System.StringSplitOptions]::RemoveEmptyEntries)[1]
        Write-Output "Container port: $ContainerPort is mapped to host port: $HostPort"

        # For each port mapping an Azure DevOps pipeline variable will be created with a name following 
        # the convention: compose.project.<COMPOSE_PROJECT_NAME>.service.<COMPOSE_SERVICE_NAME>.port.<CONTAINER_PORT>.
        # The variable value will be set to the host port.
        # Using the port mapping from above and assuming the project name is 'aspnet-core-logging-it' and 
        # the service is named 'db-v12', the following variable will be created:
        #   'compose.project.aspnet-core-logging-it.services.db-v12.ports.5432' with value: '32769'
        $VariableName = "compose.project.$ComposeProjectName.service.$($ComposeService.ServiceName).port.$ContainerPort"
        Write-Output "##vso[task.setvariable variable=$VariableName]$HostPort"
        Write-Output "##[command]Variable $VariableName has been set to: $HostPort"
        Write-Output "Finished processing port mapping: `"$RawPortMapping`"`n`n"
    }
}

docker-compose logs --tail="all" `
                    --timestamps

# Everything it's OK at this point, so exit this script the nice way :)
exit 0;