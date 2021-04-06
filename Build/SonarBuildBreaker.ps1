# Checks whether the given Git branch has passed SonarCloud quality gate via an web API call.
# 
# SonarCloud Web API
#       Methods: https://sonarcloud.io/web_api/api/
#       Authentication: https://docs.sonarqube.org/display/DEV/Web+API#WebAPI-UserToken.
Param (
    # Represents the base URL for the SonarCloud web API.
    [String]
    [Parameter(Mandatory = $true)]
    $SonarProjectKey,

    # Represents the base URL for the SonarCloud web API.
    [String]
    [Parameter(Mandatory = $true)]
    $SonarServerBaseUrl,

    # Represents the token used for authenticating against SonarCloud.
    [String]
    [Parameter(Mandatory = $true)]
    $SonarToken,

    # Represents the name of the Git branch to be checked via SonarCloud.
    [String]
    [Parameter(Mandatory = $true)]
    $GitBranchName
)

$SonarDashboardUrl = "$SonarServerBaseUrl/dashboard?id=$SonarProjectKey"

$TokenAsBytes = [System.Text.Encoding]::UTF8.GetBytes(("$SonarToken" + ":"))
$Base64Token = [System.Convert]::ToBase64String($TokenAsBytes)
$AuthorizationHeaderValue = [String]::Format("Basic {0}", $Base64Token)
$Headers = @{
    Authorization = $AuthorizationHeaderValue;
    AcceptType = "application/json"
}

# See more about the HTTP request below here: https://sonarcloud.io/web_api/api/qualitygates/project_status.
$SonarWebApiUrl = "{0}/api/qualitygates/project_status?projectKey={1}" -f $SonarServerBaseUrl, $SonarProjectKey

if ($GitBranchName.StartsWith("refs/pull/"))
{
    # The branch *is* a pull request.
    # $GitBranchName looks something like this: refs/pull/PULL_REQUEST_ID/merge (e.g. refs/pull/12/merge).
    # The interesting part is, of course, PULL_REQUEST_ID.
    $PullRequestIdWithSuffix = $GitBranchName -Replace "refs/pull/", ""
    $PullRequestId = $PullRequestIdWithSuffix -Replace "/merge", ""
    $SonarWebApiUrl = $SonarWebApiUrl + "&pullRequest=" + $PullRequestId
}
else
{
    # The branch is *not* a pull request.
    $SonarWebApiUrl = $SonarWebApiUrl + "&branch=" + $GitBranchName
}

$Response = Invoke-WebRequest -Uri $SonarWebApiUrl `
                              -Headers $Headers `
                              -UseBasicParsing `
                              -ErrorAction Stop `
                              | ConvertFrom-Json

if ($Response.projectStatus.status -eq 'OK')
{
    Write-Output "Quality gate PASSED. Please check it here: $SonarDashboardUrl"
    exit 0
}
elseif ($Response.projectStatus.status -eq 'NONE')
{
    Write-Output "There is no quality gate to be passed (yet)"
    exit 0
}

Write-Output "##vso[task.LogIssue type=error;] Quality gate FAILED. Please check it here: $SonarDashboardUrl"
Write-Output "##vso[task.complete result=Failed;]"
exit 1
