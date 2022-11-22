#!/bin/bash

# Install Docker Desktop for Mac via CLI commands.

# Fail script in case a command fails or in case of unset variables - see more here: https://www.davidpashley.com/articles/writing-robust-shell-scripts/.
set -o errexit
set -o nounset

echo 'Installing Docker ...'
brew install docker
docker info
echo 'Docker has been installed'

echo 'Installing Docker Compose ...'
brew install docker-compose
docker compose info
echo 'Docker Compose has been installed'
