# Runs a Docker container hosting the database to be targeted by the integration tests and periodically 
# checks for a given amount of tries whether the database is ready to accept incoming connections.

Param (
    # Represents the name of the Docker image to use for provisioning the database 
    # to be targeted by the integration tests.
    $DockerImageName,

    # Represents the tag associated with the Docker image to use for provisioning the database 
    # to be targeted by the integration tests.
    $DockerImageTag,

    # Represents the name of the Docker container to check whtether is running.
    $ContainerName,

    # Represents the Docker host port to use when publishing the database port.
    $HostPort,

    # Represents the database port to publish to the Docker host.
    $ContainerPort,

    # Represents the environment variables used when running the Docker container.
    $ContainerEnvironmentVariables,

    # Represents the string which occurs inside the container log signaling that 
    # the database is ready to accept incoming connections.
    $ContainerLogPatternForDatabaseReady,

    # Represents the number of milliseconds to wait before checking again whether 
    # the given container is running.
    $SleepingTimeInMillis = 250,

    # The maximum amount of retries before giving up and considering that the given 
    # Docker container is not running.
    $MaxNumberOfTries = 120
)

$ErrorActionPreference = 'Continue'

Write-Output "Pulling Docker image ${DockerImageName}:${DockerImageTag} ..."
# Success stream is redirected to null to ensure the output of the Docker command below is not printed to console
docker image pull ${DockerImageName}:${DockerImageTag} 1>$null
Write-Output "Docker image ${DockerImageName}:${DockerImageTag} has been pulled`n"

Write-Output "Starting Docker container '$ContainerName' ..."
# Success stream is redirected to null to ensure the output of the Docker command below is not printed to console
Invoke-Expression -Command "docker container run --name $ContainerName --detach --publish ${HostPort}:${ContainerPort} $ContainerEnvironmentVariables ${DockerImageName}:${DockerImageTag}" 1>$null
Write-Output "Docker container '$ContainerName' has been started"

$numberOfTries = 0
$isDatabaseReady = $false

do {
    Start-Sleep -Milliseconds $sleepingTimeInMillis

    $inspectOutput = docker inspect $ContainerName | ConvertFrom-Json 
    $containerDetails = $inspectOutput[0]
    $containerStatus = $containerDetails.State.Status

    if ($containerStatus -eq 'running') {
        # Redirect error stream to success one and set $ErrorActionPreference = 'Continue' to ensure "docker logs" command does not trick Azure DevOps into
        # thinking that this script has failed; this avoids the error: "##[error]PowerShell wrote one or more lines to the standard error stream." which
        # is reported by Azure DevOps even if the database has reached its ready state.
        $isDatabaseReady = docker logs --tail 10 $ContainerName 2>&1 | Select-String -Pattern $ContainerLogPatternForDatabaseReady -SimpleMatch -Quiet

        if ($isDatabaseReady -eq $true) {
            Write-Output "`n`nDatabase running inside container ""$ContainerName"" is ready to accept incoming connections"
            exit 0
        }
    }

    $progressMessage = "`n${numberOfTries}: Container ""$ContainerName"" isn't running yet"

    if ($numberOfTries -lt $maxNumberOfTries - 1) {
        $progressMessage += "; will check again in $sleepingTimeInMillis milliseconds"
    }
        
    Write-Output $progressMessage
    $numberOfTries++
}
until ($numberOfTries -eq $maxNumberOfTries)

# Instruct Azure DevOps to consider the current task as failed.
# See more about logging commands here: https://github.com/microsoft/azure-pipelines-tasks/blob/master/docs/authoring/commands.md.
Write-Output "##vso[task.LogIssue type=error;]Container $ContainerName is still not running after checking for $numberOfTries times; will stop here"
Write-Output "##vso[task.complete result=Failed;]"
exit 1