#!/bin/bash

# Install Docker Desktop for Mac via CLI commands.

# Fail script in case a command fails or in case of unset variables - see more here: https://www.davidpashley.com/articles/writing-robust-shell-scripts/.
set -o errexit
set -o nounset

echo 'Starting container runtime ...'
colima start
echo 'Container runtime has been started'

echo 'Installing Docker Compose ...'
# Check for the right Docker Compose version here: https://github.com/docker/compose/releases.
dockerComposeVersion='2.12.2'
sudo curl -L https://github.com/docker/compose/releases/download/v$dockerComposeVersion/docker-compose-darwin-aarch64 -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
echo 'Docker Compose has been installed'

echo 'Checking Docker and Docker Compose installations ...'
docker info
docker compose info
