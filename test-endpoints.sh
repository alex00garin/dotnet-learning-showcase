#!/bin/bash

set -e

# Start the application in Development mode on port 5249
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:5249 dotnet run --project DotnetLearningShowcase.csproj &
SERVER_PID=$!

echo "Waiting for the server to start..."
sleep 5

# Test Health Check
echo -e "\n== Health Check =="
curl -i http://localhost:5249/health

# Test Level 1 Endpoint
echo -e "\n== Level1 Weather Forecast =="
curl -i "http://localhost:5249/level1/weatherforecast?city=Berlin"

# Test Level 2 CRUD Endpoints
# 1) History for existing city (London)
echo -e "\n== Level2 History (London) =="
curl -i http://localhost:5249/level2/weatherforecast/history/London

# 2) Save new forecast
echo -e "\n== Level2 Save New Forecast =="
curl -i -X POST -H "Content-Type: application/json" -d '{"City":"TestCity","Country":"TestCountry","Forecasts":[{"Date":"2024-03-22","TemperatureC":25,"Summary":"Warm"}]}' http://localhost:5249/level2/weatherforecast/save

# 3) History for TestCity
echo -e "\n== Level2 History (TestCity) =="
curl -i http://localhost:5249/level2/weatherforecast/history/TestCity

# 4) Update summary for sample record
echo -e "\n== Level2 Update Sample Record =="
curl -i -X PUT -H "Content-Type: application/json" -d '{"Summary":"Updated Summary"}' http://localhost:5249/level2/weatherforecast/update/5fd6d4de-e0c7-4c21-9667-d555bd533aa8

# 5) Delete sample record
echo -e "\n== Level2 Delete Sample Record =="
curl -i -X DELETE http://localhost:5249/level2/weatherforecast/delete/5fd6d4de-e0c7-4c21-9667-d555bd533aa8

# 6) Final history for London
echo -e "\n== Final Level2 History (London) =="
curl -i http://localhost:5249/level2/weatherforecast/history/London

# Stop the server
echo -e "\nStopping the server..."
kill $SERVER_PID 