#!/usr/bin/env bash

# This script installs the latest version of Docker on macOS.
# See more here: https://github.com/Microsoft/azure-pipelines-image-generation/issues/738
# and here: https://github.com/microsoft/azure-pipelines-image-generation/issues/738#issuecomment-522301481.
brew cask install https://raw.githubusercontent.com/Homebrew/homebrew-cask/b8c67034bd78f9585b1316564f223b97055bc0dc/Casks/docker.rb
sudo /Applications/Docker.app/Contents/MacOS/Docker --quit-after-install --unattended
/Applications/Docker.app/Contents/MacOS/Docker --unattended &

while ! docker info 2>/dev/null ; do
  sleep 5
  echo "Waiting for Docker service to be in the running state"
done

echo "Docker service is now running"