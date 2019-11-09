# Runs a Docker container hosting the database to be targeted by the integration tests.

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

    # Represents the number of milliseconds to wait before checking again whether 
    # the given container is running.
    $SleepingTimeInMillis = 500,

    # The maximum amount of retries before giving up and considering that the given 
    # Docker container is not running.
    $MaxNumberOfTries = 60
)

$ErrorActionPreference = 'Stop'
Write-Output "ContainerEnvironmentVariables=$ContainerEnvironmentVariables"

docker image pull ${DockerImageName}:${DockerImageTag}
docker container run --name $ContainerName --detach --publish ${HostPort}:${ContainerPort} $ContainerEnvironmentVariables ${DockerImageName}:${DockerImageTag}

$numberOfTries = 1
$hasContainerStarted = $false

Do {
    Start-Sleep -Milliseconds $sleepingTimeInMillis

    $inspectOutput = docker inspect $ContainerName | ConvertFrom-Json 
    $containerDetails = $inspectOutput[0]
    $containerDetails.Config.Env | ForEach-Object {
        Write-Output "$_"
    }
    $containerStatus = $containerDetails.State.Status

    if ($containerStatus -eq 'running') {
        Write-Output "Container ""$ContainerName"" is running"
        $hasContainerStarted = $true
        break
    }

    Write-Output "#${numberOfTries}: Container ""$ContainerName"" isn't running yet; will check again in $sleepingTimeInMillis milliseconds"
    $numberOfTries++
}
Until ($numberOfTries -gt $maxNumberOfTries)

if (!$hasContainerStarted) {
        Write-Output "##vso[task.LogIssue type=error;] Container $ContainerName is not running yet; will stop here"
        Write-Output "##vso[task.complete result=Failed;]"
}
