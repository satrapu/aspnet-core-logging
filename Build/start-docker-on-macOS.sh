#!/bin/bash

# Based on: https://github.com/docker/for-mac/issues/2359#issuecomment-607154849.
# Based on: http://web.archive.org/web/20201012054023if_/https://github.com/microsoft/azure-pipelines-image-generation/issues/738#issuecomment-522301481.
# The original GitHub issue link is no longer available, thus I had to resort to the Wayback Machine URL!

# Fail script in case of unset variables - see more here:
# http://web.archive.org/web/20110314180918/http://www.davidpashley.com/articles/writing-robust-shell-scripts.html#id2577541.
set -o nounset

# Fail scripts in case a command fails - see more here:
# http://web.archive.org/web/20110314180918/http://www.davidpashley.com/articles/writing-robust-shell-scripts.html#id2577574.
set -o errexit

# Install Docker Desktop for Mac
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

brew install --cask docker &>/dev/null

# Allow Docker.app to run without confirmation
xattr -d -r com.apple.quarantine /Applications/Docker.app

# Preemptively do Docker.app's setup to avoid any GUI prompts
sudo /bin/cp /Applications/Docker.app/Contents/Library/LaunchServices/com.docker.vmnetd /Library/PrivilegedHelperTools
sudo /bin/cp /Applications/Docker.app/Contents/Resources/com.docker.vmnetd.plist /Library/LaunchDaemons/
sudo /bin/chmod 544 /Library/PrivilegedHelperTools/com.docker.vmnetd
sudo /bin/chmod 644 /Library/LaunchDaemons/com.docker.vmnetd.plist
sudo /bin/launchctl load /Library/LaunchDaemons/com.docker.vmnetd.plist
sudo /Applications/Docker.app/Contents/MacOS/Docker --quit-after-install --unattended
/Applications/Docker.app/Contents/MacOS/Docker --unattended &

end=$SECONDS
duration=$(( end - start ))
echo "Docker Desktop for Mac has been installed in $duration seconds"

printf '\n\n'
echo 'Starting Docker Desktop for Mac, if necessary ...'
echo 'Starting Docker service ...'
start=$SECONDS
open -g -a /Applications/Docker.app || exit

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

# Wait for the server to start up, if applicable.
i=0
while ! docker system info &>/dev/null; do
  (( i++ == 0 )) && printf %s 'Waiting for Docker Desktop for Mac to finish starting up ...' || printf '.'
  sleep 5
    if [[ ${retries} -gt ${maxRetries} ]]; then
        >&2 echo 'Docker service failed to enter running state during the expected time'
        exit 1
    fi;

    echo 'Waiting for Docker service to enter running state ...'
done
(( i )) && printf '\n'

end=$SECONDS
duration=$(( end - start ))
echo "Docker Desktop for Mac is ready to be used (after $duration seconds)"
echo "Docker service has started after $duration seconds"

printf '\n\n'
echo 'Displaying Docker version ...'
docker --version

printf '\n\n'
echo 'Displaying Docker Compose version ...'
docker-compose --version

printf '\n\n'
echo 'All good :)'
