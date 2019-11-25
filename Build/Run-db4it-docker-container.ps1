# Runs 'Provision-db4it-docker-container-using-XYZ.ps1' script with some hard-coded values for debugging purposes.

$ErrorActionPreference = 'Stop'

$checkReadinessStrategy = 'log-polling'
#$checkReadinessStrategy = 'healthcheck'
$ContainerName = 'db4it'
$isContainerCreated = docker container ls -a 2>&1 | Select-String -Pattern $ContainerName -SimpleMatch -Quiet

if ($isContainerCreated -eq $true) {
    Write-Output "Docker container '$ContainerName' has been created and it must be forcefully removed"
    docker container rm -f $ContainerName
    Write-Output "Docker container '$ContainerName' has been forcefully removed"
}

$DatabaseName = 'todo'
$DatabaseUserName = 'satrapu'
$DatabasePassword = 'dGUG9kfCM2S6pn3r9Qq2S7!4z7BpG4U993@DZ8GvwbMYX_j4Q7MZnNT7Jg4D6+P87FG7EmAu2fFm3BY339DpU5FhxD3s8Q4366gm'
$DatabasePort = 5432

$ScriptPath = Split-Path $MyInvocation.InvocationName
Write-Output "Will check database readiness using '$checkReadinessStrategy' strategy"

if ($checkReadinessStrategy -eq 'log-polling') {
    & $ScriptPath\Provision-db4it-container-using-log-polling.ps1 `
        -ContainerName $ContainerName `
        -HostPort 9999 `
        -ContainerPort $DatabasePort `
        -ContainerEnvironmentVariables "--env `"POSTGRES_DB=$DatabaseName`" --env `"POSTGRES_USER=$DatabaseUserName`" --env `"POSTGRES_PASSWORD=$DatabasePassword`"" `
        -DockerImageName 'stellirin/postgres-windows' `
        -DockerImageTag '11.2' `
        -ContainerLogPatternForDatabaseReady 'PostgreSQL init process complete; ready for start up.' `
        -SleepingTimeInMillis 500 `
        -MaxNumberOfTries 20
    # -DockerImageName 'postgres' `
    # -DockerImageTag '12.0-alpine' `
    #-ContainerLogPatternForDatabaseReady 'database system is ready to accept connections' `
}
else {
    & $ScriptPath\Provision-db4it-container-using-healthcheck.ps1 `
        -ContainerName $ContainerName `
        -HostPort 9999 `
        -ContainerPort $DatabasePort `
        -ContainerEnvironmentVariables "--env `"POSTGRES_DB=$DatabaseName`" --env `"POSTGRES_USER=$DatabaseUserName`" --env `"POSTGRES_PASSWORD=$DatabasePassword`"" `
        -DockerImageName 'stellirin/postgres-windows' `
        -DockerImageTag '11.2' `
        -HealthCheckCommand "pg_isready --host=localhost --port=${DatabasePort} --dbname=${DatabaseName} --username=${DatabaseUserName} --quiet" `
        -HealthCheckIntervalInMilliseconds 500 `
        -MaxNumberOfTries 20
    # -DockerImageName 'postgres' `
    # -DockerImageTag '12.0-alpine' `
}

Write-Output "Last exit code = $LastExitCode"
