#!/bin/sh
### Ensure when editing this file on Windows, its line-endings are set to "LF" instead of "CRLF"!
set -eu

main() {
    serviceName=$SERVICE_NAME
    projectName=$PROJECT_NAME
    dockerApiVersion=$DOCKER_API_VERSION
    sleepingTime="$SLEEP_BETWEEN_CONSECTUIVE_RETRIES"
    totalAttempts=$MAX_RETRIES
    currentAttempt=1
    
    echo "serviceName=$serviceName"
    echo "projectName=$projectName"
    echo "dockerApiVersion=$dockerApiVersion"
    echo "Start checking whether service \"$serviceName\" is up & running each $sleepingTime for a total amount of $totalAttempts times"

    while [ $currentAttempt -le "$totalAttempts" ]; do
        sleep "$sleepingTime"
         
        response=$(curl --silent --unix-socket /var/run/docker.sock http://v"$dockerApiVersion"/containers/json | \
                    jq '.[] 
                        | select(.Labels["com.docker.compose.project"] == "'"$projectName"'" and .Labels["com.docker.compose.service"] == "'"$serviceName"'") 
                        | select(.State == "running") 
                        | .Status
                        | contains("health")'
        )

        if [ "$response" = "true" ]; then
            echo "OK: [$currentAttempt/$totalAttempts] Service \"$serviceName\" is up & running"
            return 0
        else
            echo "WARN: [$currentAttempt/$totalAttempts] Service \"$serviceName\" is still NOT up & running"
            currentAttempt=$((currentAttempt + 1))
        fi
    done;

    echo "ERROR: Service \"$serviceName\" hasn't started in due time"
    return 1
}

main