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

    # The amount of time in milliseconds between two consecutive checks made to ensure  compose services have
    # reached healthy state.
    [Int32]
    [ValidateRange(250, [Int32]::MaxValue)]
    $HealthCheckIntervalInMilliseconds = 1000,

    # The maximum amount of retries before giving up and considering that the compose services are not running.
    [Int32]
    [ValidateRange(1, [Int32]::MaxValue)]
    $MaxNumberOfTries = 60,

    # An optional dictionary storing variables which will be passed to the containers started via Docker Compose.
    [hashtable]
    $EnvironmentVariables
)

Write-Output "Preparing to start compose services from project: $ComposeProjectName"
Write-Output "Current script path is: $PSScriptRoot"
$ComposeFilePath = [System.IO.Path]::GetFullPath((Join-Path -Path $PSScriptRoot $RelativePathToComposeFile))

if (![System.IO.File]::Exists($ComposeFilePath))
{
    Write-Output "##vso[task.LogIssue type=error;]There is no compose file at path: `"$ComposeFilePath`""
    Write-Output "##vso[task.complete result=Failed;]"
    exit 1;
}

if ($EnvironmentVariables -ne $null)
{
    $EnvironmentVariables.GetEnumerator() | ForEach-Object {
        [System.Environment]::SetEnvironmentVariable($_.Key, $_.Value, 'Process')
    }
}

$InfoMessage = "About to start compose services declared in file: `"$ComposeFilePath`" " `
              + "using project name: `"$ComposeProjectName`" ..."
Write-Output $InfoMessage

# Do not check whether this command has ended successfully since it's writing to
# standard error stream, thus tricking runtime into thinking it has failed.
docker-compose --file="$ComposeFilePath" `
               --project-name="$ComposeProjectName" `
               up `
               --detach

$LsCommandOutput = docker container ls -a `
                                    --filter "label=com.docker.compose.project=$ComposeProjectName" `
                                    --format "{{ .ID }}" `
                                    | Out-String

if ((!$?) -or ($LsCommandOutput.Length -eq 0))
{
    Write-Output "##vso[task.LogIssue type=error;]Failed to identify compose services for project: $ComposeProjectName"
    Write-Output "##vso[task.complete result=Failed;]"
    exit 4;
}

Write-Output "Found the following container(s) under compose project $($ComposeProjectName): $LsCommandOutput"

$ComposeServices = [System.Collections.Generic.List[psobject]]::new()
$LsCommandOutput.Split([System.Environment]::NewLine, [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object {
    $ContainerId = $_
    $ComposeServiceLabels = docker container inspect --format '{{ json .Config.Labels }}' `
                                                     $ContainerId `
                                                     | Out-String `
                                                     | ConvertFrom-Json

    if (!$?)
    {
        Write-Output "##vso[task.LogIssue type=error;]Failed to inspect container with ID: $ContainerId"
        Write-Output "##vso[task.complete result=Failed;]"
        exit 5;
    }

    $ComposeServiceNameLabel = 'com.docker.compose.service'
    $ComposeServiceName = $ComposeServiceLabels.$ComposeServiceNameLabel

    $ComposeService = New-Object PSObject -Property @{
        ContainerId = $ContainerId
        ServiceName = $ComposeServiceName
    }

    $ComposeServices.Add($ComposeService)

    $InfoMessage = "Found compose service with container id: `"$($ComposeService.ContainerId)`" " `
                 + "and service name: `"$($ComposeService.ServiceName)`""
    Write-Output $InfoMessage
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
        $IsServiceHealthy = docker container inspect "$($ComposeService.ContainerId)" `
                                                     --format "{{.State.Health.Status}}" `
                                                     | Select-String -Pattern 'healthy' -SimpleMatch -Quiet

        if (!$?)
        {
            Write-Output "##vso[task.LogIssue type=error;]Failed to fetch health state for compose service " `
                       + "with container id: `"$($ComposeService.ContainerId)`" " `
                       + "and service name: `"$($ComposeService.ServiceName)`" " `
                       + "from project: $ComposeProjectName"
            Write-Output "##vso[task.complete result=Failed;]"
            exit 6;
        }

        if ($IsServiceHealthy -eq $true)
        {
            Write-Output "Service with name: $($ComposeService.ServiceName) from project: $ComposeProjectName is healthy"
        }
        else
        {
            Write-Output "Service with name: $($ComposeService.ServiceName) from project: $ComposeProjectName is not healthy yet"
            $AreAllServicesReady = $false
            continue;
        }
    }

    if ($AreAllServicesReady -eq $true)
    {
        break;
    }

    $NumberOfTries++
} until ($NumberOfTries -eq $MaxNumberOfTries)

Write-Output "Finished checking heath state"

if ($AreAllServicesReady -eq $false)
{
    $ErrorMessage = "Not all services from project: $ComposeProjectName " `
                  + "are still not running after checking for $NumberOfTries times; will stop here"
    Write-Output "##vso[task.LogIssue type=error;]$ErrorMessage"
    Write-Output "##vso[task.complete result=Failed;]"
    exit 7;
}

foreach ($ComposeService in $ComposeServices)
{
    $InfoMessage = 'About to fetch port mappings for compose service ' `
                 + "with container id: `"$($ComposeService.ContainerId)`" " `
                 + "and service name: `"$($ComposeService.ServiceName)`" ..."
    Write-Output "$InfoMessage"
    $PortCommandOutput = docker container port "$($ComposeService.ContainerId)" | Out-String

    if (!$?)
    {
        $ErrorMessage = 'Failed to fetch port mappings for compose service ' `
                      + "with container id: `"$($ComposeService.ContainerId)`" " `
                      + "and service name: `"$($ComposeService.ServiceName)`""
        Write-Output "##vso[task.LogIssue type=error;]$ErrorMessage"
        Write-Output "##vso[task.complete result=Failed;]"
        exit 8;
    }

    if ($PortCommandOutput.Length -eq 0)
    {
        Write-Output "This compose service has no port mappings"
        break;
    }

    Write-Output "Found port mappings: `n$PortCommandOutput"
    $RawPortMappings = $PortCommandOutput.Split([System.Environment]::NewLine, [System.StringSplitOptions]::RemoveEmptyEntries)

    foreach ($RawPortMapping in $RawPortMappings)
    {
        if ($RawPortMapping -like '*-> ::*')
        {
            # Skip processing mappings which do not follow the proper format (e.g. <IP_ADDRESS>:<HOST_PORT>).
            # I've found a weird port mapping, 5432/tcp -> :::49153, when pipeline was running on
            # Linux-based agent; this port mapping appeared in addition to the legit one,
            # 5432/tcp -> 0.0.0.0:49153.
            Write-Output "Skip processing port mapping: `"$RawPortMapping`" since it does not look legit!"
            continue;
        }

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
        #   'compose.project.aspnet-core-logging-it.services.db-v12.port.5432' with value: '32769'
        $VariableName = "compose.project.$ComposeProjectName.service.$($ComposeService.ServiceName).port.$ContainerPort"
        Write-Output "##vso[task.setvariable variable=$VariableName]$HostPort"
        Write-Output "##[command]Variable $VariableName has been set to: $HostPort"
        Write-Output "Finished processing port mapping: `"$RawPortMapping`"`n`n"
    }
}

# Everything it's OK at this point, so exit this script the nice way :)
exit 0;
