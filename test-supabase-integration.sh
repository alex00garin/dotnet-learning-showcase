#!/bin/bash

# Test script for Supabase Integration with Level 4 Pagination
# This demonstrates how your mock_weather_data table works with the pagination system

BASE_URL="http://localhost:5249"
echo "🌤️  Testing Supabase Integration with Level 4 Pagination"
echo "========================================================="

# Test 1: Get table information
echo "📋 Test 1: Getting your Supabase table structure..."
curl -s "$BASE_URL/level4/supabase/table-info" | jq '.'
echo -e "\n"

# Test 2: Basic pagination (first page)
echo "📄 Test 2: Basic pagination (page 1, 5 items)..."
curl -s -X POST "$BASE_URL/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 5
  }' | jq '.'
echo -e "\n"

# Test 3: Filter by city
echo "🏙️  Test 3: Filter by city (Manchester)..."
curl -s -X POST "$BASE_URL/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "city": "Manchester"
  }' | jq '.'
echo -e "\n"

# Test 4: Filter by country
echo "🇬🇧 Test 4: Filter by country (UK)..."
curl -s -X POST "$BASE_URL/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 5,
    "country": "UK"
  }' | jq '.'
echo -e "\n"

# Test 5: Filter by weather summary
echo "🌧️  Test 5: Filter by weather summary (Rainy)..."
curl -s -X POST "$BASE_URL/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "summary": "Rainy"
  }' | jq '.'
echo -e "\n"

# Test 6: Filter by date range
echo "📅 Test 6: Filter by date range (July 2025)..."
curl -s -X POST "$BASE_URL/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "dateFrom": "2025-07-01",
    "dateTo": "2025-07-31"
  }' | jq '.'
echo -e "\n"

# Test 7: Filter by temperature range
echo "🌡️  Test 7: Filter by temperature range (10-20°C)..."
curl -s -X POST "$BASE_URL/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "minTemperature": 10,
    "maxTemperature": 20
  }' | jq '.'
echo -e "\n"

# Test 8: Complex filtering with sorting
echo "🔄 Test 8: Complex filtering with sorting (London, sorted by date)..."
curl -s -X POST "$BASE_URL/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "city": "London",
    "sortBy": "date",
    "sortDirection": "Ascending"
  }' | jq '.'
echo -e "\n"

# Test 9: Pagination navigation (page 2)
echo "➡️  Test 9: Navigate to page 2..."
curl -s -X POST "$BASE_URL/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 2,
    "pageSize": 3
  }' | jq '.'
echo -e "\n"

# Test 10: Multiple filters combined
echo "🎯 Test 10: Multiple filters combined (UK, Cloudy, temperature > 15)..."
curl -s -X POST "$BASE_URL/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "country": "UK",
    "summary": "Cloudy",
    "minTemperature": 15,
    "sortBy": "temperature_c",
    "sortDirection": "Descending"
  }' | jq '.'
echo -e "\n"

echo "✅ Supabase Integration Testing Complete!"
echo ""
echo "🔗 Next Steps to Connect Your Real Supabase Table:"
echo "1. Add your Supabase URL and API key to appsettings.json"
echo "2. Install Supabase NuGet package (already done)"
echo "3. Replace the mock data in Level4SupabaseEndpoints.cs with real Supabase queries"
echo "4. Use the SupabaseWeatherPaginationRequest model for your table structure"
echo ""
echo "📚 Your table has 100 rows with this structure:"
echo "   - id (uuid), city (text), country (text)"
echo "   - date (date), temperature_c (text), summary (text)"
echo ""
echo "🚀 The pagination system is universal and works with any data source!" 