#!/bin/sh
# Ensure when editing this file on Windows, its line-endings are set to "LF" instead of "CRLF"!
set -eu

main() {
    composeServiceName=$COMPOSE_SERVICE_NAME
    composeProjectName=$COMPOSE_PROJECT_NAME
    dockerApiVersion=$DOCKER_API_VERSION
    sleepBetweenConsecutiveRetries="$SLEEP_BETWEEN_CONSECUTIVE_RETRIES"
    maxRetries=$MAX_RETRIES
    
    if [ "$DEBUG" = "true" ]; then
      printf "Debug mode is *on*\n"
      printf "composeServiceName=%s\n" "$composeServiceName"
      printf "composeProjectName=%s\n" "$composeProjectName"
      printf "dockerApiVersion=%s\n" "$dockerApiVersion"
      printf "sleepBetweenConsecutiveRetries=%s\n" "$sleepBetweenConsecutiveRetries"
      printf "maxRetries=%s\n" "$maxRetries"
    else
      printf "Debug mode is *off*\n"
    fi
    
    printf "\n"
    printf "Start checking whether service \"%s\" is up & running each %s for a total amount of %d times\n\n" \
              "$composeServiceName" "$sleepBetweenConsecutiveRetries" "$maxRetries"

    currentAttempt=1
    
    while [ $currentAttempt -le "$maxRetries" ]; do
        printf "Trial %d/%d\n" "$currentAttempt" "$maxRetries"
        sleep "$sleepBetweenConsecutiveRetries"
        
        # Request the list of specific compose services from Docker Engine API.
        # See more about curl command here: https://curl.haxx.se/docs/manpage.html.
        dockerEngineApiResponse=$(curl --silent \
                                       --get \
                                       --unix-socket /var/run/docker.sock \
                                       --data-urlencode 'all=true' \
                                       --data-urlencode "filters={\"label\":[\"com.docker.compose.project=$composeProjectName\", \"com.docker.compose.service=$composeServiceName\"]}" \
                                       http://v"$dockerApiVersion"/containers/json)
        
        if [ "$DEBUG" = "true" ]; then
          printf "Docker Engine API response is: \n%s\n" "$dockerEngineApiResponse"
        fi  
        
        # Process the Docker Engine API response in order to find whether the given compose service is healthy or not.
        # See more about jq command here: https://stedolan.github.io/jq/manual/.
        # Please keep in mind the Docker Engine API response is different than the outcome of the `docker inspect` command.
        isComposeServiceHealthy=$(echo "$dockerEngineApiResponse" | \
                    jq '.[] 
                        | select(.State == "running") 
                        | .Status 
                        | contains("healthy")')
            
        if [ "$isComposeServiceHealthy" = "true" ]; then
            printf "%-4s: Service \"%s\" is up & running" "OK" "$composeServiceName"
            printf "\n%s\n\n" "--------------------------------------------------"
            return 0
        else
            printf "%-4s: Service \"%s\" is still *not* up & running" "WARN" "$composeServiceName"
            printf "\n%s\n\n" "--------------------------------------------------"
            currentAttempt=$((currentAttempt + 1))
        fi
    done;

    printf "ERROR: Service \"%s\" hasn't started in due time\n" "$composeServiceName"
    return 1
}

main