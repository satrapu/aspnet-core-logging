#!/bin/bash

# Install Docker on macOS via CLI commands.

# Fail script in case a command fails or in case of unset variables - see more here: https://www.davidpashley.com/articles/writing-robust-shell-scripts/.
set -o errexit
set -o nounset

# Check for the right Docker Compose version here: https://github.com/docker/compose/releases.
# Installation steps can be found here: https://docs.docker.com/compose/install/standalone/.
dockerComposeVersion='2.30.3'

# Check for how to customize Colima VM here: https://github.com/abiosoft/colima?tab=readme-ov-file#customizing-the-vm.
colimaCpuCount=2
colimaMemorySizeInGigabytes=2
colimaDiskSizeInGigabytes=10

# Install Docker CLI
echo "Installing Docker CLI ..."
brew install docker
echo 'Checking Docker CLI installation ...'
docker --version
echo "Docker CLI has been installed successfully"

# Install Docker Compose
echo "Installing Docker Compose with version: $dockerComposeVersion ..."
sudo curl -L https://github.com/docker/compose/releases/download/v$dockerComposeVersion/docker-compose-darwin-x86_64 -o /usr/local/bin/docker-compose -v
sudo chmod +x /usr/local/bin/docker-compose
echo 'Checking Docker Compose installation ...'
docker-compose version
echo "Docker Compose has been installed successfully"

# Install qemu (a Colima prerequisite)
# satrapu 2024-11-09: Temporarily install qemu due to https://github.com/abiosoft/colima/issues/1188.
# Once Colima v0.70.0 is available to brew, qemu should not longer be installed.
brew install qemu

# Install Colima
brew install colima

# Start Colima container runtime.
# Check for Colima usage here: https://github.com/abiosoft/colima?tab=readme-ov-file#usage.
echo 'Starting Colima container runtime ...'
colima start --cpu $colimaCpuCount --memory $colimaMemorySizeInGigabytes --disk $colimaDiskSizeInGigabytes
echo 'Colima container runtime has started'
echo 'All good :)'
