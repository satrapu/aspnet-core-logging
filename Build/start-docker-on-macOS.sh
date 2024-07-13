#!/bin/bash

# Install Docker on macOS via CLI commands.

# Fail script in case a command fails or in case of unset variables - see more here: https://www.davidpashley.com/articles/writing-robust-shell-scripts/.
set -o errexit
set -o nounset

echo 'Installing Docker client ...'
brew install docker
echo 'Docker client has been installed'

echo 'Installing Docker Compose ...'
# Check for the right Docker Compose version here: https://github.com/docker/compose/releases.
dockerComposeVersion='2.28.1'
sudo curl -L https://github.com/docker/compose/releases/download/v$dockerComposeVersion/docker-compose-darwin-x86_64 -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
echo 'Docker Compose has been installed'

echo 'Starting container runtime ...'
colima start
echo 'Container runtime has started'

echo 'Checking Docker installation ...'
docker info

echo 'Checking Docker Compose installation ...'
docker-compose version

echo 'All good :)'
