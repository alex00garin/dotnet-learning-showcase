#!/bin/bash

# Check if level is provided as an argument
if [ $# -eq 0 ]; then
  echo "Usage: ./deploy-to-fly.sh <level> [app-name]"
  echo "Example: ./deploy-to-fly.sh 2 dotnet-weather-crud"
  echo "Example: ./deploy-to-fly.sh 3 my-new-app"
  exit 1
fi

LEVEL=$1
APP_NAME=${2:-"dotnet-level$LEVEL-app"}

echo "Building and deploying Level$LEVEL to Fly.io as $APP_NAME..."

# Check if the level directory exists
if [ ! -d "Level$LEVEL" ]; then
  echo "Error: Level$LEVEL directory does not exist."
  exit 1
fi

# Check if fly.toml exists in the level directory
if [ ! -f "Level$LEVEL/fly.toml" ]; then
  echo "Error: fly.toml not found in Level$LEVEL directory."
  echo "You need to create a Fly.io app first with:"
  echo "cd Level$LEVEL && fly launch --name $APP_NAME"
  exit 1
fi

# Build the Docker image and deploy to Fly.io
echo "Deploying to Fly.io..."
cd "Level$LEVEL"
fly deploy --app $APP_NAME

echo "Deployment complete! Your app is available at https://$APP_NAME.fly.dev/" 