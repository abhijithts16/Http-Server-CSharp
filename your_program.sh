#!/bin/sh
#
# Use this script to run your program LOCALLY.
#
set -e # Exit early if any commands fail

#
# - Edit this to change how your program compiles locally
(
  cd "$(dirname "$0")" # Ensure compile steps are run within the repository directory
  dotnet build --configuration Release --output /tmp/build-http-server-csharp http-server.csproj
)

#
# - Edit this to change how your program runs locally
exec /tmp/build-http-server-csharp/http-server "$@"
