#!/bin/sh
### Ensure when editing this file on Windows, its line-endings are set to "LF" instead of "CRLF"!
set -eu

main() {
    serviceName=$SERVICE_NAME
    projectName=$COMPOSE_PROJECT_NAME
    dockerApiVersion=$DOCKER_API_VERSION
    sleepingTime="$SLEEP_BETWEEN_CONSECTUIVE_RETRIES"
    totalAttempts=$MAX_RETRIES
    currentAttempt=1
    
    if [ "$DEBUG" = "true" ]; then
      printf "DEBUG is *on*\n"
      printf "serviceName=%s\n" "$serviceName"
      printf "dockerApiVersion=%s\n" "$dockerApiVersion"
      printf "\n\n\n"
    fi
    
    printf "Start checking whether service \"%s\" is up & running each %s for a total amount of %d times\n" \
              "$serviceName" "$sleepingTime" "$totalAttempts"

    while [ $currentAttempt -le "$totalAttempts" ]; do
        sleep "$sleepingTime"
        
        response=$(curl --silent --unix-socket /var/run/docker.sock http://v"$dockerApiVersion"/containers/json | \
                    jq '.[] 
                        | select(.Labels["com.docker.compose.project"] == "'"$projectName"'")
                        | select(.Labels["com.docker.compose.service"] == "'"$serviceName"'")
                        | select(.State == "running") 
                        | .Status 
                        | contains("healthy")')
            
        if [ "$DEBUG" = "true" ]; then
          printf "Docker Engine API response is: \n%s\n\n" "$response"
        fi    
                  
        if [ "$response" = "true" ]; then
            printf "%-4s: [%d/%d] Service \"%s\" is up & running\n" "OK" "$currentAttempt" "$totalAttempts" "$serviceName"
            return 0
        else
            printf "%-4s: [%d/%d] Service \"%s\" is still *not* up & running\n" "WARN" "$currentAttempt" "$totalAttempts" "$serviceName"
            currentAttempt=$((currentAttempt + 1))
        fi
    done;

    printf "ERROR: Service \"%s\" hasn't started in due time\n" "$serviceName"
    return 1
}

main