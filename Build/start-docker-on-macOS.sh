#!/bin/bash

# Based on: https://github.com/microsoft/azure-pipelines-image-generation/issues/738#issuecomment-527013065.
echo "About to install Docker ..."

# Install working Docker version 2.0.0.3-ce-mac81,31259
brew cask install https://raw.githubusercontent.com/Homebrew/homebrew-cask/8ce4e89d10716666743b28c5a46cd54af59a9cc2/Casks/docker.rb
sudo /Applications/Docker.app/Contents/MacOS/Docker --quit-after-install --unattended
/Applications/Docker.app/Contents/MacOS/Docker --unattended &
echo "Docker has been installed"

# Waiting for Docker service to start
echo "Waiting for Docker service to be in the running state ..."
retries=0

while ! docker info 2>/dev/null ; do
    sleep 5
    retries=[[expr ${retries} + 1]]
    
    if pgrep -xq -- "Docker"; then
        echo 'Docker still running'
    else
        echo 'Docker not running, restart'
        /Applications/Docker.app/Contents/MacOS/Docker --unattended &
    fi
    
    if [[ ${retries} -gt 30 ]]; then
        >&2 echo 'Failed to run Docker'
        exit 1
    fi;

    echo 'Waiting for Docker service to be in the running state'
done

echo "Docker service is now running"

# Install Docker Compose
echo "About to install Docker Compose ..."
brew install docker-compose
echo "Docker Compose has been installed"