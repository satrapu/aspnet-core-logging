#!/usr/bin/env bash

brew cask install docker
sudo /Applications/Docker.app/Contents/MacOS/Docker --quit-after-install --unattended
/Applications/Docker.app/Contents/MacOS/Docker --unattended &

while ! docker info 2>/dev/null ; do
  sleep 5
  echo "Waiting for Docker service to be in the running state"
done

echo "Docker service is now running"