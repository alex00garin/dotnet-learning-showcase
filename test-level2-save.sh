#!/bin/bash

echo "Testing Level 2 Save Endpoint..."
echo "================================="

# Test 1: Valid request
echo "Test 1: Valid request"
echo "---------------------"
curl -X POST \
  -H "Content-Type: application/json" \
  -H "Origin: https://www.alexandergarin.com" \
  -d '{
    "city": "London", 
    "country": "UK",
    "forecasts": [
      {
        "date": "2024-01-15",
        "temperatureC": 20,
        "summary": "Sunny test weather"
      },
      {
        "date": "2024-01-16", 
        "temperatureC": 18,
        "summary": "Cloudy test weather"
      }
    ]
  }' \
  https://api.alexandergarin.com/level2/weatherforecast/save

echo -e "\n\n"

# Test 2: CORS preflight request 
echo "Test 2: CORS preflight request"
echo "------------------------------"
curl -X OPTIONS \
  -H "Origin: https://www.alexandergarin.com" \
  -H "Access-Control-Request-Method: POST" \
  -H "Access-Control-Request-Headers: Content-Type" \
  -i \
  https://api.alexandergarin.com/level2/weatherforecast/save

echo -e "\n\n"

# Test 3: Invalid request (missing city)
echo "Test 3: Invalid request (missing city)"
echo "--------------------------------------"
curl -X POST \
  -H "Content-Type: application/json" \
  -H "Origin: https://www.alexandergarin.com" \
  -d '{
    "country": "UK",
    "forecasts": [
      {
        "date": "2024-01-15",
        "temperatureC": 20,
        "summary": "Test weather"
      }
    ]
  }' \
  https://api.alexandergarin.com/level2/weatherforecast/save

echo -e "\n\n"

# Test 4: localhost testing
echo "Test 4: Testing localhost API"
echo "-----------------------------"
curl -X POST \
  -H "Content-Type: application/json" \
  -d '{
    "city": "Berlin",
    "country": "Germany", 
    "forecasts": [
      {
        "date": "2024-01-15",
        "temperatureC": 22,
        "summary": "Local test weather"
      }
    ]
  }' \
  http://localhost:5000/level2/weatherforecast/save

echo -e "\n\n"

# Test 5: Check if API is running
echo "Test 5: Health check"
echo "--------------------"
curl -i https://api.alexandergarin.com/health

echo -e "\n\nTest complete!" 