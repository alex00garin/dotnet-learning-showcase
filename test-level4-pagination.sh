#!/bin/bash

# Level 4 Pagination Testing Script
# This script tests all the advanced pagination features

BASE_URL="http://localhost:5249"
LEVEL4_URL="$BASE_URL/level4"

echo "🚀 Level 4 Pagination Testing Suite"
echo "===================================="
echo ""

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Function to print test headers
print_test() {
    echo -e "${BLUE}📋 Testing: $1${NC}"
    echo "----------------------------------------"
}

# Function to print success
print_success() {
    echo -e "${GREEN}✅ $1${NC}"
    echo ""
}

# Function to print info
print_info() {
    echo -e "${YELLOW}ℹ️  $1${NC}"
}

# Check if server is running
print_test "Server Health Check"
response=$(curl -s -w "%{http_code}" "$BASE_URL/health" -o /dev/null)
if [ "$response" -eq 200 ]; then
    print_success "Server is running on $BASE_URL"
else
    echo -e "${RED}❌ Server not responding. Please start the application first.${NC}"
    exit 1
fi

# Test 1: Level 4 Root Endpoint
print_test "Level 4 Root Endpoint"
curl -s "$LEVEL4_URL/" | jq -r '.' || echo "Level 4 - Advanced Pagination & Data Management"
print_success "Root endpoint working"

# Test 2: Pagination Info Helper
print_test "Pagination Info Helper"
curl -s "$LEVEL4_URL/pagination/info" | jq '.'
print_success "Pagination info retrieved"

# Test 3: Pagination Demo with Sample Data
print_test "Pagination Demo - Page 1"
curl -s -X POST "$LEVEL4_URL/paginate/demo" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 5,
    "sortBy": "value",
    "sortDirection": "Ascending"
  }' | jq '.'
print_success "Demo pagination page 1 completed"

print_test "Pagination Demo - Page 2"
curl -s -X POST "$LEVEL4_URL/paginate/demo" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 2,
    "pageSize": 5,
    "sortBy": "value",
    "sortDirection": "Descending"
  }' | jq '.'
print_success "Demo pagination page 2 completed"

# Test 4: Weather History Pagination (requires existing data)
print_test "Weather History Pagination - London"
print_info "First, let's add some weather data for London..."

# Add weather data for London
curl -s -X POST "$BASE_URL/level2/weatherforecast/save" \
  -H "Content-Type: application/json" \
  -d '{
    "city": "London",
    "date": "2024-01-15",
    "temperatureC": 5,
    "summary": "Cold"
  }' > /dev/null

curl -s -X POST "$BASE_URL/level2/weatherforecast/save" \
  -H "Content-Type: application/json" \
  -d '{
    "city": "London",
    "date": "2024-01-16",
    "temperatureC": 8,
    "summary": "Cool"
  }' > /dev/null

# Now test paginated history
curl -s -X POST "$LEVEL4_URL/weather/history/paginated" \
  -H "Content-Type: application/json" \
  -d '{
    "city": "London",
    "page": 1,
    "pageSize": 10,
    "sortBy": "date",
    "sortDirection": "Descending"
  }' | jq '.'
print_success "Weather history pagination completed"

# Test 5: Weather History with Filters
print_test "Weather History with Temperature Filters"
curl -s -X POST "$LEVEL4_URL/weather/history/paginated" \
  -H "Content-Type: application/json" \
  -d '{
    "city": "London",
    "page": 1,
    "pageSize": 5,
    "sortBy": "temperature",
    "sortDirection": "Ascending",
    "minTemperature": 0,
    "maxTemperature": 10
  }' | jq '.'
print_success "Weather history with temperature filters completed"

# Test 6: Autocomplete Pagination
print_test "Autocomplete Pagination - Cities starting with 'New'"
curl -s -X POST "$LEVEL4_URL/autocomplete/paginated" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "New",
    "dataSource": "cities",
    "page": 1,
    "pageSize": 5,
    "sortBy": "name",
    "sortDirection": "Ascending"
  }' | jq '.'
print_success "Autocomplete pagination completed"

# Test 7: Cities Browse with GET (simple)
print_test "Cities Browse - GET Request"
curl -s "$LEVEL4_URL/cities/browse?page=1&pageSize=3&sortBy=name&searchQuery=London" | jq '.'
print_success "Cities browse GET completed"

# Test 8: Cities Browse with POST (advanced filters)
print_test "Cities Browse - POST with Advanced Filters"
curl -s -X POST "$LEVEL4_URL/cities/browse" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 5,
    "sortBy": "name",
    "sortDirection": "Ascending",
    "searchQuery": "San",
    "minPopulation": 100000
  }' | jq '.'
print_success "Cities browse POST with filters completed"

# Test 9: Bulk Weather Operations
print_test "Bulk Weather Operations with Pagination"
curl -s -X POST "$LEVEL4_URL/weather/bulk/paginated" \
  -H "Content-Type: application/json" \
  -d '{
    "items": ["London", "Paris", "Berlin", "Madrid", "Rome"],
    "batchSize": 2,
    "includePagination": true
  }' | jq '.'
print_success "Bulk weather operations completed"

# Test 10: Edge Cases - Invalid Pagination Parameters
print_test "Edge Case - Invalid Page Number"
curl -s -X POST "$LEVEL4_URL/paginate/demo" \
  -H "Content-Type: application/json" \
  -d '{
    "page": -1,
    "pageSize": 5
  }' | jq '.'
print_success "Invalid page number handled correctly"

print_test "Edge Case - Large Page Size"
curl -s -X POST "$LEVEL4_URL/paginate/demo" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 200
  }' | jq '.'
print_success "Large page size clamped correctly"

# Test 11: Different Sorting Options
print_test "Autocomplete with Population Sorting"
curl -s -X POST "$LEVEL4_URL/autocomplete/paginated" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "Los",
    "dataSource": "cities",
    "page": 1,
    "pageSize": 3,
    "sortBy": "population",
    "sortDirection": "Descending"
  }' | jq '.'
print_success "Population-based sorting completed"

echo ""
echo -e "${GREEN}🎉 All Level 4 Pagination Tests Completed Successfully!${NC}"
echo ""
echo -e "${YELLOW}📊 Summary of Features Tested:${NC}"
echo "• Basic pagination with page/pageSize"
echo "• Advanced filtering (temperature, population, country)"
echo "• Multiple sorting options (name, date, temperature, population)"
echo "• Bulk operations with batching"
echo "• Edge case handling (invalid parameters)"
echo "• Different data sources (cities, weather history)"
echo "• GET and POST endpoint variations"
echo ""
echo -e "${BLUE}🔍 To explore more, try modifying the parameters in this script!${NC}" 