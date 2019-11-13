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

$ErrorActionPreference = 'Stop'

docker image pull ${DockerImageName}:${DockerImageTag}
Invoke-Expression -Command "docker container run --name $ContainerName --detach --publish ${HostPort}:${ContainerPort} $ContainerEnvironmentVariables ${DockerImageName}:${DockerImageTag}"

$numberOfTries = 0
$isDatabaseReady = $false

do {
    Start-Sleep -Milliseconds $sleepingTimeInMillis

    $inspectOutput = docker inspect $ContainerName | ConvertFrom-Json 
    $containerDetails = $inspectOutput[0]
    $containerStatus = $containerDetails.State.Status

    if ($containerStatus -eq 'running') {
        $isDatabaseReady = docker logs --tail 10 $ContainerName | Select-String -Pattern $ContainerLogPatternForDatabaseReady -SimpleMatch -Quiet
        
        if ($isDatabaseReady) {
            Write-Output "`n`nDatabase running inside container ""$ContainerName"" is ready to accept incoming connections"
            exit 0
        }
    }

    $progressMessage = "${numberOfTries}: Container ""$ContainerName"" isn't running yet"

    if ($numberOfTries -lt $maxNumberOfTries - 1) {
        $progressMessage += "; will check again in $sleepingTimeInMillis milliseconds"
    }
        
    Write-Output "$progressMessage`n`n"
    $numberOfTries++
}
until ($numberOfTries -eq $maxNumberOfTries)

if (!$isDatabaseReady) {
    Write-Output "##vso[task.LogIssue type=error;] Container $ContainerName is still not running after checking for $numberOfTries times; will stop here"
    Write-Output "##vso[task.complete result=Failed;]"
    exit 1
}