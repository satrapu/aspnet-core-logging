# This script saves the compose service log files to a given folder.
Param(
    # Docker Compose project name.
    # See more here: https://docs.docker.com/compose/reference/overview/#use--p-to-specify-a-project-name.
    [String]
    $ComposeProjectName = 'aspnet-core-logging-it',

    # The path to the folder where Docker Compose will write its logs.
    [String]
    $LogsOutputFolder
)

$InfoMessage = "The log files of compose services from project: $ComposeProjectName " `
             + "will be written to folder: `"$LogsOutputFolder`""
Write-Output "$InfoMessage"

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

$LsCommandOutput.Split([System.Environment]::NewLine, [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object {
    $ContainerId = $_
    $ComposeServiceLabelsAsJson = docker inspect --format '{{ json .Config.Labels }}' `
                                                 "$ContainerId" `
                                                 | Out-String `
                                                 | ConvertFrom-Json

    if (!$?)
    {
        Write-Output "##vso[task.LogIssue type=error;]Failed to inspect container with ID: $ContainerId"
        Write-Output "##vso[task.complete result=Failed;]"
        exit 2;
    }

    $ComposeServiceNameLabel = 'com.docker.compose.service'
    $ComposeServiceName = $ComposeServiceLabelsAsJson.$ComposeServiceNameLabel
    $LogFileName = "$ComposeProjectName--$ComposeServiceName--$ContainerId.log"
    $LogFilePath = Join-Path -Path $LogsOutputFolder $LogFileName

    $PublishLogsInfoMessage = "About to publish logs for compose service with container id: " `
                            + "`"$ContainerId`" and service name: " `
                            + "`"$ComposeServiceName`" to file: `"$LogFilePath`" ..."
    Write-Output $PublishLogsInfoMessage

    if (![System.IO.File]::Exists($LogsOutputFolder))
    {
       New-Item -Path "$LogsOutputFolder" -ItemType "Directory"
    }

    # Do not check whether this command has ended successfully since it's writing to 
    # standard error stream, thus tricking runtime into thinking it failed.
    docker logs --tail "all" `
                --details `
                --timestamps `
                "$ContainerId" `
                | Out-File -FilePath "$LogFilePath"
}

exit 0;