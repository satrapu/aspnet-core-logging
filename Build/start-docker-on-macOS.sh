#!/bin/bash

# Install Docker Desktop for Mac via its native command line support.
# This script is based on:https://docs.docker.com/desktop/install/mac-install/#install-from-the-command-line.

# Fail script in case of unset variables - see more here:
# http://web.archive.org/web/20110314180918/http://www.davidpashley.com/articles/writing-robust-shell-scripts.html#id2577541.
set -o nounset

# Fail scripts in case a command fails - see more here:
# http://web.archive.org/web/20110314180918/http://www.davidpashley.com/articles/writing-robust-shell-scripts.html#id2577574.
set -o errexit

echo 'Downloading Docker Desktop for Mac installation file ...'
start=$SECONDS
curl -O -L https://desktop.docker.com/mac/main/amd64/Docker.dmg
end=$SECONDS
duration=$(( end - start ))
echo "Installation file has been downloaded in $duration seconds"

echo 'Installing Docker Desktop for Mac ...'
start=$SECONDS
sudo hdiutil attach Docker.dmg
sudo /Volumes/Docker/Docker.app/Contents/MacOS/install --user=runner --accept-license
sudo hdiutil detach /Volumes/Docker
end=$SECONDS
duration=$(( end - start ))
echo "Docker Desktop for Mac has been installed in $duration seconds"

docker --version
docker compose --version
