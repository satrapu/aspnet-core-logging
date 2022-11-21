#!/bin/bash

# Install Docker Desktop for Mac via its native command line support.
# This script is based on:https://docs.docker.com/desktop/install/mac-install/#install-from-the-command-line.

# Fail script in case of unset variables - see more here:
# http://web.archive.org/web/20110314180918/http://www.davidpashley.com/articles/writing-robust-shell-scripts.html#id2577541.
set -o nounset

# Fail scripts in case a command fails - see more here:
# http://web.archive.org/web/20110314180918/http://www.davidpashley.com/articles/writing-robust-shell-scripts.html#id2577574.
set -o errexit

echo 'Listing all users ...'
ls /Users
echo 'Users have been listed'

echo 'Downloading Docker Desktop for Mac installation file ...'
start=$SECONDS
curl -O -L https://desktop.docker.com/mac/main/amd64/Docker.dmg
end=$SECONDS
duration=$(( end - start ))
echo "Installation file has been downloaded in $duration seconds"

echo 'Installing Docker Desktop for Mac ...'
start=$SECONDS
userNameForRunningDocker=runner
sudo hdiutil attach Docker.dmg
sudo /Volumes/Docker/Docker.app/Contents/MacOS/install --user=$userNameForRunningDocker --accept-license
sudo hdiutil detach /Volumes/Docker
end=$SECONDS
duration=$(( end - start ))
echo "Docker Desktop for Mac has been installed in $duration seconds"

waitTimeInSeconds=5
maxRetries=30
retries=0

echo 'Starting Docker service ...'
start=$SECONDS

while [[ ${retries} -lt ${maxRetries} ]]; do
    sleep $waitTimeInSeconds

    echo 'Checking whether Docker service has started ...'
    docker info || true > /dev/null 2>&1

    if [[ "$?" -eq 0 ]]; then
        break
    fi;
    
    ((retries=retries+1))
done

if [[ ${retries} -gt ${maxRetries} ]]; then
    >&2 echo 'ERROR: Docker service failed to start during the expected time'
    exit 1
fi;

end=$SECONDS
duration=$(( end - start ))
echo "OK: Docker service has started after $duration seconds"

echo 'Displaying Docker & Docker Compose versions ...'
docker version
docker compose version
echo 'All good - start running some containers on this machine'
