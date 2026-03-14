#!/bin/bash

# Run Docker containers on macOS with the help of Colima.

# Fail script in case a command fails or in case of unset variables - see more here: https://www.davidpashley.com/articles/writing-robust-shell-scripts/.
set -o errexit
set -o nounset

export PATH="/usr/local/bin:$PATH"

# Ensure Lima does not run.
limactl stop || true

# Ensure Colima does not run.
colima stop || true

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

# Check for the right Lima version here: https://github.com/lima-vm/lima/releases.
LIMA_VERSION="2.0.3"
LIMA_ARCH="x86_64"
LIMA_OS_NAME="Darwin"
LIMA_INSTALL_DIR="$(mktemp -d)"

# Check for the right Colima version here: https://github.com/abiosoft/colima/releases.
COLIMA_VERSION="0.10.1"
COLIMA_ARCH="x86_64"
COLIMA_OS_NAME="Darwin"

# Check for how to customize Colima VM here: https://github.com/abiosoft/colima?tab=readme-ov-file#customizing-the-vm.
COLIMA_CPU_COUNT=2
COLIMA_MEMORY_SIZE_IN_GIGABYTES=2
COLIMA_DISK_SIZE_IN_GIGABYTES=5

# Install Docker CLI.
echo "Installing Docker CLI with version: ${DOCKER_CLI_VERSION} ..."
echo "Downloading Docker CLI archive ..."
curl -L "${DOCKER_CLI_DOWNLOAD_BASE_URL}/${DOCKER_CLI_TAR_FILE}" -o "${DOCKER_CLI_INSTALL_DIR}/${DOCKER_CLI_TAR_FILE}"
echo "Extracting Docker CLI archive ..."
tar -xzf "${DOCKER_CLI_INSTALL_DIR}/${DOCKER_CLI_TAR_FILE}" --strip-components=1 -C "${DOCKER_CLI_INSTALL_DIR}"
mv "${DOCKER_CLI_INSTALL_DIR}/docker" /usr/local/bin/docker
chmod +x /usr/local/bin/docker
echo 'Checking Docker CLI installation ...'
docker --version
echo "Docker CLI has been installed successfully"
printf "\n\n\n"

# Install Docker Compose.
echo "Installing Docker Compose with version: ${DOCKER_COMPOSE_VERSION} ..."
echo "Downloading Docker Compose archive ..."
curl -L "https://github.com/docker/compose/releases/download/v${DOCKER_COMPOSE_VERSION}/docker-compose-${DOCKER_COMPOSE_OS_NAME}-${DOCKER_COMPOSE_ARCH}" \
          -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose
echo 'Checking Docker Compose installation ...'
docker-compose version
echo "Docker Compose has been installed successfully"
printf "\n\n\n"

# Install Lima (required for Colima).
echo "Installing Lima with version: ${LIMA_VERSION} ..."
echo "Downloading Lima archive ..."
curl -L "https://github.com/lima-vm/lima/releases/download/v${LIMA_VERSION}/lima-${LIMA_VERSION}-${LIMA_OS_NAME}-${LIMA_ARCH}.tar.gz" \
          -o "${LIMA_INSTALL_DIR}/lima.tar.gz"
echo "Extracting Lima archive ..."
tar -xzf "${LIMA_INSTALL_DIR}/lima.tar.gz" --strip-components=1 -C "${LIMA_INSTALL_DIR}"
mv "${LIMA_INSTALL_DIR}/bin/limactl" /usr/local/bin/limactl
chmod +x /usr/local/bin/limactl
mkdir -p /usr/local/share
cp -r "${LIMA_INSTALL_DIR}/share/lima" /usr/local/share/lima
## If the Lima tarball contains the Lima binary, install it as well.
if [ -f "${LIMA_INSTALL_DIR}/bin/lima" ]; then
  mv "${LIMA_INSTALL_DIR}/bin/lima" /usr/local/bin/lima
  chmod +x /usr/local/bin/lima
fi
echo 'Checking Lima installation ...'
limactl --version
if command -v lima >/dev/null 2>&1; then
  lima --version
else
  echo "Warning: Lima binary not found in PATH. Colima may report an error if Lima is unavailable."
fi
echo "Lima has been installed successfully"
printf "\n\n\n"

# Install Colima.
echo "Installing Colima with version: ${COLIMA_VERSION} ..."
echo "Downloading Colima archive ..."
curl -L "https://github.com/abiosoft/colima/releases/download/v${COLIMA_VERSION}/colima-${COLIMA_OS_NAME}-${COLIMA_ARCH}" \
          -o /usr/local/bin/colima
chmod +x /usr/local/bin/colima
echo 'Checking Colima installation ...'
colima version
echo "Colima has been installed successfully"
printf "\n\n\n"

# Start Colima.
# Check for Colima usage here: https://github.com/abiosoft/colima?tab=readme-ov-file#usage.
echo 'Starting Colima container runtime ...'
env PATH="/usr/local/bin:$PATH" colima start --cpu $COLIMA_CPU_COUNT --memory $COLIMA_MEMORY_SIZE_IN_GIGABYTES --disk $COLIMA_DISK_SIZE_IN_GIGABYTES
echo 'Colima container runtime has started'
printf "\n\n\n"
echo 'All good :)'
