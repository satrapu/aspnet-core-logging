#!/bin/bash

# Based on: https://github.com/microsoft/azure-pipelines-image-generation/issues/738#issuecomment-527013065.
brew cask install docker
sudo /Applications/Docker.app/Contents/MacOS/Docker --quit-after-install --unattended
/Applications/Docker.app/Contents/MacOS/Docker --unattended &

retries=0
maxRetries=30

while ! docker info 2>/dev/null ; do
    sleep 5s
    ((retries=retries+1))
    
    if pgrep -xq -- "Docker"; then
        echo 'Docker service still running'
    else
        echo 'Docker service not running, restart'
        /Applications/Docker.app/Contents/MacOS/Docker --unattended &
    fi
    
    if [[ ${retries} -gt ${maxRetries} ]]; then
        >&2 echo 'Failed to start Docker service'
        exit 1
    fi;

    echo 'Waiting for Docker service to enter running state'
done

echo "Docker service is now running"