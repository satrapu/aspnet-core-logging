#!/bin/bash

# Based on: http://web.archive.org/web/20201012054023if_/https://github.com/microsoft/azure-pipelines-image-generation/issues/738#issuecomment-522301481.
# The original GitHub issue link is no longer available, thus I had to resort to the Wayback Machine URL!

# Fail script in case of unset variables - see more here:
# http://web.archive.org/web/20110314180918/http://www.davidpashley.com/articles/writing-robust-shell-scripts.html#id2577541.
set -o nounset

# Fail scripts in case a command fails - see more here:
# http://web.archive.org/web/20110314180918/http://www.davidpashley.com/articles/writing-robust-shell-scripts.html#id2577574.
set -o errexit

# Install Docker Desktop for Mac via brew tool
echo 'Downloading and then running docker brew formula ...'
start=$SECONDS

# The brew formula below will install Docker Desktop for Mac, v2.0.0.3,31259.
dockerInstallationScriptName='docker.rb'
dockerInstallationScriptUrl="https://raw.githubusercontent.com/Homebrew/homebrew-cask/8ce4e89d10716666743b28c5a46cd54af59a9cc2/Casks/$dockerInstallationScriptName"
curl -L  $dockerInstallationScriptUrl > $dockerInstallationScriptName && brew install $dockerInstallationScriptName

end=$SECONDS
duration=$(( end - start ))
echo "Docker brew formula has been downloaded & run in $duration seconds"

echo 'Installing Docker Desktop for Mac ...'
start=$SECONDS
sudo /Applications/Docker.app/Contents/MacOS/Docker --quit-after-install --unattended
/Applications/Docker.app/Contents/MacOS/Docker --unattended &

end=$SECONDS
duration=$(( end - start ))
echo "Docker Desktop for Mac has been installed in $duration seconds"

echo 'Starting Docker service ...'
start=$SECONDS

retries=0
maxRetries=30

while ! docker info 2>/dev/null ; do
    sleep 5s
    ((retries=retries+1))

    if pgrep -xq -- 'Docker'; then
        echo 'Docker service is still booting'
    else
        echo 'Docker service is no longer running, need to restart it'
        /Applications/Docker.app/Contents/MacOS/Docker --unattended &
    fi

    if [[ ${retries} -gt ${maxRetries} ]]; then
        >&2 echo 'Docker service failed to enter running state during the expected time'
        exit 1
    fi;

    echo 'Waiting for Docker service to enter running state ...'
done

end=$SECONDS
duration=$(( end - start ))
echo "Docker service has started after $duration seconds"

docker --version
docker-compose --version
