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

$TokenAsBytes = [System.Text.Encoding]::UTF8.GetBytes(("$SonarToken" + ":"))
$Base64Token = [System.Convert]::ToBase64String($TokenAsBytes)
$AuthorizationHeaderValue = [String]::Format("Basic {0}", $Base64Token)
$Headers = @{
    Authorization = $AuthorizationHeaderValue;
    AcceptType = "application/json"
}

# See mor about the HTTP request below here: https://sonarcloud.io/web_api/api/qualitygates/project_status.
$SonarWebApiUrl = "{0}/api/qualitygates/project_status?projectKey={1}&branch={2}" -f $SonarServerBaseUrl, $SonarProjectKey, $GitBranchName
$Response = Invoke-WebRequest -Uri $SonarWebApiUrl -Headers $Headers -UseBasicParsing -ErrorAction Stop | ConvertFrom-Json

if ($Response.projectStatus.status -eq 'OK')
{
    Write-Output "Quality gate PASSED. Please check it here: {0}/dashboard?id={1}" -f $SonarServerBaseUrl, $SonarProjectKey
    exit 0
}
elseif ($Response.projectStatus.status -eq 'NONE')
{
    Write-Output "There is no quality gate to be passed (yet)"
    exit 0
}

$ErrorMsg = "##vso[task.LogIssue type=error;] Quality gate FAILED. Please check it here: {0}/dashboard?id={1}" -f $SonarServerBaseUrl, $SonarProjectKey
Write-Output $ErrorMsg
Write-Output "##vso[task.complete result=Failed;]"
exit 1
