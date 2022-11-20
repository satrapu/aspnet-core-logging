#!/bin/bash

# Install Docker Desktop for Mac via brew tool.
# This script is based on: https://github.com/docker/for-mac/issues/2359#issuecomment-991942550.

# Fail script in case of unset variables - see more here:
# http://web.archive.org/web/20110314180918/http://www.davidpashley.com/articles/writing-robust-shell-scripts.html#id2577541.
set -o nounset

# Fail scripts in case a command fails - see more here:
# http://web.archive.org/web/20110314180918/http://www.davidpashley.com/articles/writing-robust-shell-scripts.html#id2577574.
set -o errexit

echo 'Installing Docker Desktop for Mac ...'
start=$SECONDS

brew install --cask docker
docker_app_path=$(brew list --cask docker | grep '==> App' -A1 | tail -n 1 | awk '{ print $1 }')
docker_app_path="${docker_app_path/#\~/$HOME}"

sudo "$docker_app_path"/Contents/MacOS/Docker --unattended --install-privileged-components
open -a "$docker_app_path" --args --unattended --accept-license
while ! "$docker_app_path"/Contents/Resources/bin/docker info &>/dev/null; do sleep 1; done

end=$SECONDS
duration=$(( end - start ))
echo "Docker Desktop for Mac has been installed & started after $duration seconds"

docker --version
docker compose --version
