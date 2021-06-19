#!/bin/bash

# This script is based on https://github.com/docker/for-mac/issues/2359#issuecomment-853420567.

echo 'Installing Docker ...'
start=$SECONDS

# Install Docker v20.10.6
dockerInstallationScriptName='docker.rb'
gitCommitSha='62aced99afd24aacd9003a7593b6210f3fdb8a65'
dockerInstallationScriptUrl="https://raw.githubusercontent.com/Homebrew/homebrew-cask/$gitCommitSha/Casks/$dockerInstallationScriptName"
curl -L  $dockerInstallationScriptUrl > $dockerInstallationScriptName && brew install $dockerInstallationScriptName

end=$SECONDS
duration=$(( end - start ))
echo "Docker has been installed in $duration seconds"

# allow the app to run without confirmation
xattr -d -r com.apple.quarantine /Applications/Docker.app

# preemptively do docker.app's setup to avoid any gui prompts
sudo /bin/cp /Applications/Docker.app/Contents/Library/LaunchServices/com.docker.vmnetd /Library/PrivilegedHelperTools

# the plist we need used to be in /Applications/Docker.app/Contents/Resources, but
# is now dynamically generated. So we dynamically generate our own:
sudo tee "/Library/LaunchDaemons/com.docker.vmnetd.plist" > /dev/null <<'EOF'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
	<key>Label</key>
	<string>com.docker.vmnetd</string>
	<key>Program</key>
	<string>/Library/PrivilegedHelperTools/com.docker.vmnetd</string>
	<key>ProgramArguments</key>
	<array>
		<string>/Library/PrivilegedHelperTools/com.docker.vmnetd</string>
	</array>
	<key>RunAtLoad</key>
	<true/>
	<key>Sockets</key>
	<dict>
		<key>Listener</key>
		<dict>
			<key>SockPathMode</key>
			<integer>438</integer>
			<key>SockPathName</key>
			<string>/var/run/com.docker.vmnetd.sock</string>
		</dict>
	</dict>
	<key>Version</key>
	<string>59</string>
</dict>
</plist>

EOF

sudo /bin/chmod 544 /Library/PrivilegedHelperTools/com.docker.vmnetd
sudo /bin/chmod 644 /Library/LaunchDaemons/com.docker.vmnetd.plist
sudo /bin/launchctl load /Library/LaunchDaemons/com.docker.vmnetd.plist

echo 'Starting Docker.app, if necessary ...'
start=$SECONDS

sleep 5s
open -g -a Docker.app || exit

# Wait for the server to start up, if applicable.
i=0

while ! docker system info &>/dev/null; do
  (( i++ == 0 )) && printf %s 'Waiting for Docker to finish starting up...' || printf '.'
  sleep 3s
done

(( i )) && printf '\n'

end=$SECONDS
duration=$(( end - start ))
echo "Docker has started in $duration seconds"

docker --version
docker-compose --version
