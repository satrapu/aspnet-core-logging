#!/bin/bash

# Based on: https://github.com/docker/for-mac/issues/2359#issuecomment-607154849.

# Fail script in case of unset variables - see more here: 
# http://web.archive.org/web/20110314180918/http://www.davidpashley.com/articles/writing-robust-shell-scripts.html#id2577541.
set -o nounset
 
# Fail scripts in case a command fails - see more here:
# http://web.archive.org/web/20110314180918/http://www.davidpashley.com/articles/writing-robust-shell-scripts.html#id2577574.
set -o errexit

# Install latest version of Docker Desktop for Mac
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

end=$SECONDS
duration=$(( end - start ))
echo "Docker Desktop for Mac has been installed in $duration seconds"

printf '\n\n'
echo 'Starting Docker Desktop for Mac, if necessary ...'
start=$SECONDS
open -g -a Docker.app || exit

# Wait for the server to start up, if applicable.
i=0
while ! docker system info &>/dev/null; do
  (( i++ == 0 )) && printf %s 'Waiting for Docker Desktop for Mac to finish starting up ...' || printf '.'
  sleep 5
done
(( i )) && printf '\n'

end=$SECONDS
duration=$(( end - start ))
echo "Docker Desktop for Mac is ready to be used (after $duration seconds)"

printf '\n\n'
echo 'Displaying Docker version ...'
docker --version

printf '\n\n'
echo 'Displaying Docker Compose version ...'
docker-compose --version

printf '\n\n'
echo 'All good :)'
