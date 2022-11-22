#!/bin/bash

# Install Docker Desktop for Mac via CLI commands.

# Fail script in case a command fails or in case of unset variables - see more here: https://www.davidpashley.com/articles/writing-robust-shell-scripts/.
set -o errexit
set -o nounset

echo 'Installing Docker ...'
brew install --cask docker
docker info
docker compose info
echo 'Docker has been installed'
