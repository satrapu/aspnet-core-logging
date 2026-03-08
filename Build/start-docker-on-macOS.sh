#!/bin/bash

# Install Docker on macOS via CLI commands.

# Fail script in case a command fails or in case of unset variables - see more here: https://www.davidpashley.com/articles/writing-robust-shell-scripts/.
set -o errexit
set -o nounset

# Check for the right Docker CLI version here: https://github.com/docker/cli/tags.
DOCKER_CLI_VERSION="29.3.0"
DOCKER_CLI_ARCH="x86_64"
DOCKER_CLI_DOWNLOAD_BASE_URL="https://download.docker.com/mac/static/stable/${DOCKER_CLI_ARCH}"
DOCKER_CLI_TAR_FILE="docker-${DOCKER_CLI_VERSION}.tgz"
DOCKER_CLI_INSTALL_DIR="$(mktemp -d)"

# Check for the right Docker Compose version here: https://github.com/docker/compose/releases.
# Installation steps can be found here: https://docs.docker.com/compose/install/standalone/.
DOCKER_COMPOSE_VERSION='5.1.0'
DOCKER_COMPOSE_ARCH="x86_64"
DOCKER_COMPOSE_OS_NAME="darwin"

# Check for the right Colima version here: https://github.com/abiosoft/colima/releases.
COLIMA_VERSION="0.10.1"
COLIMA_ARCH="x86_64"
COLIMA_OS_NAME="Darwin"

# Check for how to customize Colima VM here: https://github.com/abiosoft/colima?tab=readme-ov-file#customizing-the-vm.
colimaCpuCount=2
colimaMemorySizeInGigabytes=2
colimaDiskSizeInGigabytes=10

# Install Docker CLI
echo "Installing Docker CLI with version: ${DOCKER_CLI_VERSION} ..."
echo "Downloading Docker CLI archive ..."
curl -L "${DOCKER_CLI_DOWNLOAD_BASE_URL}/${DOCKER_CLI_TAR_FILE}" -o "${DOCKER_CLI_INSTALL_DIR}/${DOCKER_CLI_TAR_FILE}"
echo "Extracting Docker CLI archive ..."
tar -xzf "${DOCKER_CLI_INSTALL_DIR}/${DOCKER_CLI_TAR_FILE}" -C "${DOCKER_CLI_INSTALL_DIR}"
sudo mv "${DOCKER_CLI_INSTALL_DIR}/docker/docker" /usr/local/bin/docker
sudo chmod +x /usr/local/bin/docker
echo 'Checking Docker CLI installation ...'
docker --version
echo "Docker CLI has been installed successfully"
printf "\n\n\n"

# Install Docker Compose
echo "Installing Docker Compose with version: ${DOCKER_COMPOSE_VERSION} ..."
echo "Downloading Docker Compose archive ..."
sudo curl -L "https://github.com/docker/compose/releases/download/v${DOCKER_COMPOSE_VERSION}/docker-compose-${DOCKER_COMPOSE_OS_NAME}-${DOCKER_COMPOSE_ARCH}" -o
/usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
echo 'Checking Docker Compose installation ...'
docker-compose version
echo "Docker Compose has been installed successfully"
printf "\n\n\n"

# Install Colima
echo "Installing Colima with version: ${COLIMA_VERSION} ..."
echo "Downloading Colima archive ..."
curl -L "https://github.com/abiosoft/colima/releases/download/v${COLIMA_VERSION}/colima-${COLIMA_OS_NAME}-${COLIMA_ARCH}" -o /usr/local/bin/colima
chmod +x /usr/local/bin/colima
echo 'Checking Colima installation ...'
colima version
echo "Colima has been installed successfully"
printf "\n\n\n"

# Start Colima container runtime.
# Check for Colima usage here: https://github.com/abiosoft/colima?tab=readme-ov-file#usage.
echo 'Starting Colima container runtime ...'
colima start --cpu $colimaCpuCount --memory $colimaMemorySizeInGigabytes --disk $colimaDiskSizeInGigabytes
echo 'Colima container runtime has started'
printf "\n\n\n"
echo 'All good :)'
