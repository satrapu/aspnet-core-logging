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

echo 'Starting Docker service ...'
start=$SECONDS

retries=0
maxRetries=30

while ! docker info 2>/dev/null ; do
    sleep 5s
    ((retries=retries+1))

    if [[ ${retries} -gt ${maxRetries} ]]; then
        >&2 echo 'Docker service failed to enter running state during the expected time'
        exit 1
    fi;

    echo 'Waiting for Docker service to enter running state ...'
done

end=$SECONDS
duration=$(( end - start ))
echo "Docker service has started after $duration seconds"

echo 'Displaying Docker & Docker Compose versions ...'
docker version
docker compose version
echo 'All good - start running some containers on this machine'
