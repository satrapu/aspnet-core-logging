#!/bin/bash

# Install Docker Desktop for Mac via CLI commands.

# Fail script in case a command fails or in case of unset variables - see more here: https://www.davidpashley.com/articles/writing-robust-shell-scripts/.
set -o errexit
set -o nounset

echo 'Installing container runtime ...'
brew install colima
echo 'Container runtime has been installed'

echo 'Installing Docker Compose ...'
dockerComposeVersion='2.12.2'
sudo curl -L https://github.com/docker/compose/releases/download/v$dockerComposeVersion/docker-compose-darwin-aarch64 -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose
echo 'Docker Compose has been installed'

echo 'Checking Docker and Docker Compose installations ...'
docker info
docker compose info
