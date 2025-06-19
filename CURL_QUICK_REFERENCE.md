# 🚀 Level 4 Pagination - Curl Quick Reference

## Essential Curl Commands for Testing Level 4 Pagination

### 1. Basic Health Check
```bash
curl -s http://localhost:5249/health
```

### 2. Level 4 Root & Info
```bash
# Root endpoint
curl -s http://localhost:5249/level4/

# Pagination info helper
curl -s http://localhost:5249/level4/pagination/info | jq '.'
```

### 3. Pagination Demo (Sample Data)
```bash
# Page 1, 5 items, sorted by value ascending
curl -s -X POST http://localhost:5249/level4/paginate/demo \
  -H "Content-Type: application/json" \
  -d '{"page": 1, "pageSize": 5, "sortBy": "value", "sortDirection": "Ascending"}' | jq '.'

# Page 2, different sorting
curl -s -X POST http://localhost:5249/level4/paginate/demo \
  -H "Content-Type: application/json" \
  -d '{"page": 2, "pageSize": 3, "sortBy": "date", "sortDirection": "Descending"}' | jq '.'
```

### 4. Cities Autocomplete with Pagination
```bash
# Search cities starting with "New"
curl -s -X POST http://localhost:5249/level4/autocomplete/paginated \
  -H "Content-Type: application/json" \
  -d '{"query": "New", "dataSource": "cities", "page": 1, "pageSize": 3, "sortBy": "name", "sortDirection": "Ascending"}' | jq '.'

# Search with population sorting (if available)
curl -s -X POST http://localhost:5249/level4/autocomplete/paginated \
  -H "Content-Type: application/json" \
  -d '{"query": "Los", "dataSource": "cities", "page": 1, "pageSize": 3, "sortBy": "population", "sortDirection": "Descending"}' | jq '.'
```

### 5. Cities Browse (GET vs POST)
```bash
# Simple GET request
curl -s "http://localhost:5249/level4/cities/browse?page=1&pageSize=3&sortBy=name&searchQuery=London" | jq '.'

# Advanced POST with filters
curl -s -X POST http://localhost:5249/level4/cities/browse \
  -H "Content-Type: application/json" \
  -d '{"page": 1, "pageSize": 5, "sortBy": "name", "sortDirection": "Ascending", "searchQuery": "San", "minPopulation": 100000}' | jq '.'
```

### 6. Weather History Pagination
```bash
# First, add some weather data
curl -s -X POST http://localhost:5249/level2/weatherforecast/save \
  -H "Content-Type: application/json" \
  -d '{"city": "London", "date": "2024-01-15", "temperatureC": 5, "summary": "Cold"}' > /dev/null

# Then paginate the history
curl -s -X POST http://localhost:5249/level4/weather/history/paginated \
  -H "Content-Type: application/json" \
  -d '{"city": "London", "page": 1, "pageSize": 10, "sortBy": "date", "sortDirection": "Descending"}' | jq '.'

# With temperature filters
curl -s -X POST http://localhost:5249/level4/weather/history/paginated \
  -H "Content-Type: application/json" \
  -d '{"city": "London", "page": 1, "pageSize": 5, "sortBy": "temperature", "sortDirection": "Ascending", "minTemperature": 0, "maxTemperature": 10}' | jq '.'
```

### 7. Bulk Weather Operations
```bash
# Process multiple cities with batching
curl -s -X POST http://localhost:5249/level4/weather/bulk/paginated \
  -H "Content-Type: application/json" \
  -d '{"items": ["London", "Paris", "Berlin"], "batchSize": 2, "includePagination": true}' | jq '.'
```

### 8. Edge Cases & Validation
```bash
# Invalid page number (gets corrected to 1)
curl -s -X POST http://localhost:5249/level4/paginate/demo \
  -H "Content-Type: application/json" \
  -d '{"page": -1, "pageSize": 5}' | jq '.pagination'

# Large page size (gets clamped to 100)
curl -s -X POST http://localhost:5249/level4/paginate/demo \
  -H "Content-Type: application/json" \
  -d '{"page": 1, "pageSize": 200}' | jq '.pagination'
```

## 📊 Understanding the Response Structure

Every paginated response includes:

```json
{
  "data": [...],           // Array of actual items
  "pagination": {
    "currentPage": 1,      // Current page number
    "pageSize": 10,        // Items per page
    "totalItems": 157,     // Total items available
    "totalPages": 16,      // Total pages available
    "hasNextPage": true,   // Can navigate forward
    "hasPreviousPage": false, // Can navigate backward
    "firstItemIndex": 1,   // Index of first item on page
    "lastItemIndex": 10    // Index of last item on page
  }
}
```

## 🎯 Quick Testing Tips

1. **Start the server**: `dotnet run --project DotnetLearningShowcase.csproj`
2. **Use jq for formatting**: Pipe curl output to `| jq '.'` for pretty JSON
3. **Test pagination flow**: Start with page 1, then try page 2, 3, etc.
4. **Try different sorting**: Use `"sortBy": "name"`, `"date"`, `"temperature"`, `"population"`
5. **Test filters**: Add `minPopulation`, `maxTemperature`, `countryFilter`, etc.
6. **Edge cases**: Try page -1, page 9999, pageSize 0, pageSize 1000

## 🔧 Advanced Usage

### Pagination Navigation
```bash
# Get page info first
PAGE_INFO=$(curl -s "http://localhost:5249/level4/pagination/info?totalItems=157&page=5&pageSize=10")
echo $PAGE_INFO | jq '.metadata'

# Navigate through pages
for page in {1..3}; do
  echo "=== Page $page ==="
  curl -s -X POST http://localhost:5249/level4/paginate/demo \
    -H "Content-Type: application/json" \
    -d "{\"page\": $page, \"pageSize\": 5}" | jq '.pagination'
done
```

### Performance Testing
```bash
# Test with different page sizes
for size in 5 10 25 50 100; do
  echo "Testing pageSize: $size"
  time curl -s -X POST http://localhost:5249/level4/paginate/demo \
    -H "Content-Type: application/json" \
    -d "{\"page\": 1, \"pageSize\": $size}" > /dev/null
done
```

---

**💡 Pro Tip**: Run `./test-level4-pagination.sh` for a comprehensive test suite that covers all endpoints!

## 🌟 Supabase Weather Data Endpoints

### Supabase Weather Pagination - Basic
```bash
# Basic weather data pagination (localhost)
curl -X POST "http://localhost:5249/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 8,
    "sortBy": "city",
    "sortDirection": "Ascending"
  }'

# Production
curl -X POST "https://api.alexandergarin.com/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 8,
    "sortBy": "city",
    "sortDirection": "Ascending"
  }'
```

### Supabase Weather - City Filter
```bash
# Filter by city (localhost)
curl -X POST "http://localhost:5249/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "city": "London",
    "sortBy": "date",
    "sortDirection": "Descending"
  }'

# Production
curl -X POST "https://api.alexandergarin.com/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "city": "London",
    "sortBy": "date",
    "sortDirection": "Descending"
  }'
```

### Supabase Weather - Weather Summary Filter
```bash
# Filter by weather summary (localhost)
curl -X POST "http://localhost:5249/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "summary": "Rainy",
    "sortBy": "temperatureC",
    "sortDirection": "Ascending"
  }'

# Production
curl -X POST "https://api.alexandergarin.com/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "summary": "Rainy",
    "sortBy": "temperatureC",
    "sortDirection": "Ascending"
  }'
```

### Supabase Weather - Temperature Range Filter
```bash
# Filter by temperature range (localhost)
curl -X POST "http://localhost:5249/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 15,
    "minTemperature": 10,
    "maxTemperature": 20,
    "sortBy": "temperatureC",
    "sortDirection": "Descending"
  }'

# Production
curl -X POST "https://api.alexandergarin.com/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 15,
    "minTemperature": 10,
    "maxTemperature": 20,
    "sortBy": "temperatureC",
    "sortDirection": "Descending"
  }'
```

### Supabase Weather - Date Range Filter
```bash
# Filter by date range (localhost)
curl -X POST "http://localhost:5249/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "dateFrom": "2025-07-01",
    "dateTo": "2025-07-15",
    "sortBy": "date",
    "sortDirection": "Ascending"
  }'

# Production
curl -X POST "https://api.alexandergarin.com/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "dateFrom": "2025-07-01",
    "dateTo": "2025-07-15",
    "sortBy": "date",
    "sortDirection": "Ascending"
  }'
```

### Supabase Weather - Combined Filters
```bash
# Multiple filters combined (localhost)
curl -X POST "http://localhost:5249/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 5,
    "country": "UK",
    "summary": "Cloudy",
    "minTemperature": 5,
    "maxTemperature": 25,
    "dateFrom": "2025-06-01",
    "dateTo": "2025-07-31",
    "sortBy": "city",
    "sortDirection": "Ascending"
  }'

# Production
curl -X POST "https://api.alexandergarin.com/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 5,
    "country": "UK",
    "summary": "Cloudy",
    "minTemperature": 5,
    "maxTemperature": 25,
    "dateFrom": "2025-06-01",
    "dateTo": "2025-07-31",
    "sortBy": "city",
    "sortDirection": "Ascending"
  }'
```

### Supabase Table Info
```bash
# Get table structure info (localhost)
curl -X GET "http://localhost:5249/level4/supabase/table-info"

# Production
curl -X GET "https://api.alexandergarin.com/level4/supabase/table-info"
```

## 📈 Supabase Response Format

All Supabase endpoints return weather data in this format:

```json
{
  "data": [
    {
      "id": "1",
      "city": "Manchester",
      "country": "UK",
      "date": "2025-07-02T00:00:00",
      "temperatureC": "11",
      "summary": "Rainy"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 8,
    "totalItems": 10,
    "totalPages": 2,
    "hasNextPage": true,
    "hasPreviousPage": false,
    "firstItemIndex": 1,
    "lastItemIndex": 8
  }
}
```

## 🔧 Available Supabase Filter Options

- `city` - Filter by city name (partial match)
- `country` - Filter by country (partial match)  
- `summary` - Filter by weather summary (partial match)
- `dateFrom` - Filter by start date (YYYY-MM-DD)
- `dateTo` - Filter by end date (YYYY-MM-DD)
- `minTemperature` - Minimum temperature filter
- `maxTemperature` - Maximum temperature filter

## 🔃 Available Sort Options

- `city` - Sort by city name
- `country` - Sort by country
- `date` - Sort by weather date
- `temperatureC` - Sort by temperature
- `summary` - Sort by weather summary
  }'
```

### 4. Autocomplete Pagination
```bash
# Enhanced autocomplete with filtering
curl -X POST "http://localhost:5249/level4/autocomplete/paginated" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "New",
    "dataSource": "cities",
    "page": 1,
    "pageSize": 20,
    "countryFilter": "United States"
  }'
```

### 5. Bulk Weather Operations
```bash
# Bulk weather with batch processing
curl -X POST "http://localhost:5249/level4/weather/bulk/paginated" \
  -H "Content-Type: application/json" \
  -d '{
    "items": ["London", "Paris", "Berlin", "Madrid", "Rome"],
    "page": 1,
    "pageSize": 10,
    "batchSize": 2
  }'
```

## Supabase Weather Data Endpoints

### 6. Supabase Weather Pagination - Basic
```bash
# Basic weather data pagination (localhost)
curl -X POST "http://localhost:5249/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 8,
    "sortBy": "city",
    "sortDirection": "Ascending"
  }'

# Production
curl -X POST "https://api.alexandergarin.com/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 8,
    "sortBy": "city",
    "sortDirection": "Ascending"
  }'
```

### 7. Supabase Weather - City Filter
```bash
# Filter by city (localhost)
curl -X POST "http://localhost:5249/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "city": "London",
    "sortBy": "date",
    "sortDirection": "Descending"
  }'

# Production
curl -X POST "https://api.alexandergarin.com/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "city": "London",
    "sortBy": "date",
    "sortDirection": "Descending"
  }'
```

### 8. Supabase Weather - Weather Summary Filter
```bash
# Filter by weather summary (localhost)
curl -X POST "http://localhost:5249/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "summary": "Rainy",
    "sortBy": "temperatureC",
    "sortDirection": "Ascending"
  }'

# Production
curl -X POST "https://api.alexandergarin.com/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "summary": "Rainy",
    "sortBy": "temperatureC",
    "sortDirection": "Ascending"
  }'
```

### 9. Supabase Weather - Temperature Range Filter
```bash
# Filter by temperature range (localhost)
curl -X POST "http://localhost:5249/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 15,
    "minTemperature": 10,
    "maxTemperature": 20,
    "sortBy": "temperatureC",
    "sortDirection": "Descending"
  }'

# Production
curl -X POST "https://api.alexandergarin.com/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 15,
    "minTemperature": 10,
    "maxTemperature": 20,
    "sortBy": "temperatureC",
    "sortDirection": "Descending"
  }'
```

### 10. Supabase Weather - Date Range Filter
```bash
# Filter by date range (localhost)
curl -X POST "http://localhost:5249/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "dateFrom": "2025-07-01",
    "dateTo": "2025-07-15",
    "sortBy": "date",
    "sortDirection": "Ascending"
  }'

# Production
curl -X POST "https://api.alexandergarin.com/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "dateFrom": "2025-07-01",
    "dateTo": "2025-07-15",
    "sortBy": "date",
    "sortDirection": "Ascending"
  }'
```

### 11. Supabase Weather - Combined Filters
```bash
# Multiple filters combined (localhost)
curl -X POST "http://localhost:5249/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 5,
    "country": "UK",
    "summary": "Cloudy",
    "minTemperature": 5,
    "maxTemperature": 25,
    "dateFrom": "2025-06-01",
    "dateTo": "2025-07-31",
    "sortBy": "city",
    "sortDirection": "Ascending"
  }'

# Production
curl -X POST "https://api.alexandergarin.com/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 5,
    "country": "UK",
    "summary": "Cloudy",
    "minTemperature": 5,
    "maxTemperature": 25,
    "dateFrom": "2025-06-01",
    "dateTo": "2025-07-31",
    "sortBy": "city",
    "sortDirection": "Ascending"
  }'
```

### 12. Supabase Table Info
```bash
# Get table structure info (localhost)
curl -X GET "http://localhost:5249/level4/supabase/table-info"

# Production
curl -X GET "https://api.alexandergarin.com/level4/supabase/table-info"
```

## Pagination Metadata Helper
```bash
# Calculate pagination metadata
curl -X GET "http://localhost:5249/level4/pagination/info?totalItems=100&page=2&pageSize=10"

# Production
curl -X GET "https://api.alexandergarin.com/level4/pagination/info?totalItems=100&page=2&pageSize=10"
```

## Expected Response Format

All endpoints return data in this consistent format:

```json
{
  "data": [
    {
      "id": "1",
      "city": "Manchester",
      "country": "UK",
      "date": "2025-07-02T00:00:00",
      "temperatureC": "11",
      "summary": "Rainy"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 8,
    "totalItems": 10,
    "totalPages": 2,
    "hasNextPage": true,
    "hasPreviousPage": false,
    "firstItemIndex": 1,
    "lastItemIndex": 8
  }
}
```

## Available Sort Options for Supabase Weather

- `city` - Sort by city name
- `country` - Sort by country
- `date` - Sort by weather date
- `temperatureC` - Sort by temperature
- `summary` - Sort by weather summary

## Available Filter Options for Supabase Weather

- `city` - Filter by city name (partial match)
- `country` - Filter by country (partial match)
- `summary` - Filter by weather summary (partial match)
- `dateFrom` - Filter by start date (YYYY-MM-DD)
- `dateTo` - Filter by end date (YYYY-MM-DD)
- `minTemperature` - Minimum temperature filter
- `maxTemperature` - Maximum temperature filter

## Sort Direction Options

- `Ascending` - Sort from low to high / A to Z
- `Descending` - Sort from high to low / Z to A 