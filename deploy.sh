#!/bin/bash

APP_NAME="dotnet-learning-showcase"

echo "Building and deploying DotnetLearningShowcase to Fly.io as $APP_NAME..."

# Check if the app exists in Fly.io
if ! fly status --app $APP_NAME &>/dev/null; then
  echo "Creating new Fly.io app: $APP_NAME"
  fly launch --name $APP_NAME --no-deploy
else
  echo "Using existing Fly.io app: $APP_NAME"
fi

# Deploy to Fly.io
echo "Deploying to Fly.io..."
fly deploy --app $APP_NAME

echo "Deployment complete! Your app is available at https://$APP_NAME.fly.dev/"
echo "Level 1 API is at: https://$APP_NAME.fly.dev/level1/weatherforecast"
echo "Level 2 API is at: https://$APP_NAME.fly.dev/level2/weatherforecast/history/London" 