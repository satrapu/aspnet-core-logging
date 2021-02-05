#!/bin/bash

# Based on: https://github.com/microsoft/azure-pipelines-image-generation/issues/738#issuecomment-527013065.

# Fail script in case of unset variables - see more here: 
# http://web.archive.org/web/20110314180918/http://www.davidpashley.com/articles/writing-robust-shell-scripts.html#id2577541.
set -o nounset
 
# Fail scripts in case a command fails - see more here:
# http://web.archive.org/web/20110314180918/http://www.davidpashley.com/articles/writing-robust-shell-scripts.html#id2577574.
set -o errexit

# Install latest version of Docker Desktop for Mac
echo 'Installing Docker Desktop for Mac ...'
brew install --cask docker
echo 'Docker Desktop for Mac has been installed'

echo 'Checking Docker version ...'
docker --version

echo 'Checking Docker Compose version ...'
docker-compose --version

echo 'All good :)'
