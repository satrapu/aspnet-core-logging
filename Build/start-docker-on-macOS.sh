#!/bin/bash

# Install Docker on macOS via CLI commands.

# Fail script in case a command fails or in case of unset variables - see more here: https://www.davidpashley.com/articles/writing-robust-shell-scripts/.
set -o errexit
set -o nounset

# Check for the right Docker CLI version here: https://github.com/docker/cli/tags.
dockerCliVersion='27.0.3'

# Check for the right Docker Compose version here: https://github.com/docker/compose/releases.
# Installation steps can be found here: https://docs.docker.com/compose/install/standalone/.
dockerComposeVersion='2.28.1'

echo "Installing Docker CLI with version: $dockerCliVersion ..."
# Check for installing a specific Homebrew formula version here: https://blog.sandipb.net/2021/09/02/installing-a-specific-version-of-a-homebrew-formula/.
localDockerCliHomeFolder=$USER/local-docker-cli
brew tap-new "$localDockerCliHomeFolder"
brew extract --version=$dockerCliVersion docker "$localDockerCliHomeFolder"
HOMEBREW_NO_AUTO_UPDATE=1 brew install "$localDockerCliHomeFolder@$dockerCliVersion"
echo 'Checking Docker installation ...'
docker --version
echo "Docker CLI v$dockerCliVersion has been installed successfully"

echo "Installing Docker Compose with version: $dockerComposeVersion ..."
sudo curl -L https://github.com/docker/compose/releases/download/v$dockerComposeVersion/docker-compose-darwin-x86_64 -o /usr/local/bin/docker-compose -v
sudo chmod +x /usr/local/bin/docker-compose
echo 'Checking Docker Compose installation ...'
docker-compose version
echo "Docker Compose v$dockerComposeVersion has been installed successfully"

# Colima container runtime usage can be found here: https://github.com/abiosoft/colima?tab=readme-ov-file#usage.
echo 'Starting Colima container runtime ...'
colima start
echo 'Colima container runtime has started'
echo 'All good :)'
