# Level 4 - Advanced Pagination & Data Management

## 🚀 Overview

Level 4 introduces comprehensive pagination capabilities to the .NET Learning Showcase, building upon the previous levels with advanced data management features including filtering, sorting, batch processing, and intelligent pagination metadata.

## 🔧 Key Features

### ✨ **Smart Pagination**
- **Automatic validation**: Page and size parameters are validated and clamped to safe ranges
- **Rich metadata**: Comprehensive pagination information with navigation helpers
- **Flexible page sizes**: Support for 1-100 items per page with intelligent defaults

### 🔍 **Advanced Filtering**
- **Date range filtering**: Filter weather history by date ranges
- **Temperature filtering**: Min/max temperature constraints
- **Country filtering**: Filter cities and countries by location
- **Population filtering**: Filter cities by population ranges
- **Search filtering**: Text-based search across multiple fields

### 📊 **Multi-criteria Sorting**
- **Weather data**: Sort by date, temperature, city, summary
- **Autocomplete data**: Sort by name, country, population, relevance
- **Bidirectional**: Ascending/descending support for all fields

### ⚡ **Batch Processing**
- **Bulk operations**: Process multiple items in configurable batches
- **Progress tracking**: Detailed batch information and progress metadata
- **Error resilience**: Graceful handling of individual item failures

## 📋 API Endpoints

### 1. Paginated Weather History
```http
POST /level4/weather/history/paginated
Content-Type: application/json

{
  "city": "Berlin",
  "page": 1,
  "pageSize": 10,
  "sortBy": "date",
  "sortDirection": "Descending",
  "dateFrom": "2024-01-01T00:00:00Z",
  "dateTo": "2024-12-31T23:59:59Z",
  "minTemperature": 0,
  "maxTemperature": 35
}
```

**Response:**
```json
{
  "data": [
    {
      "id": "guid",
      "city": "Berlin",
      "country": "Germany",
      "date": "2024-01-15T00:00:00Z",
      "temperatureC": 22,
      "summary": "Sunny"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalItems": 157,
    "totalPages": 16,
    "hasNextPage": true,
    "hasPreviousPage": false,
    "firstItemIndex": 1,
    "lastItemIndex": 10
  }
}
```

### 2. Enhanced Autocomplete Pagination
```http
POST /level4/autocomplete/paginated
Content-Type: application/json

{
  "query": "Lon",
  "dataSource": "cities",
  "page": 1,
  "pageSize": 5,
  "sortBy": "name",
  "sortDirection": "Ascending",
  "countryFilter": "GB",
  "minPopulation": 100000
}
```

### 3. Cities Browse (GET & POST)
```http
GET /level4/cities/browse?page=1&pageSize=20&sortBy=name&countryFilter=US&searchQuery=New

POST /level4/cities/browse
Content-Type: application/json

{
  "page": 1,
  "pageSize": 20,
  "sortBy": "population",
  "sortDirection": "Descending",
  "countryFilter": "US",
  "searchQuery": "New",
  "minPopulation": 50000,
  "maxPopulation": 1000000
}
```

### 4. Bulk Weather Operations
```http
POST /level4/weather/bulk/paginated
Content-Type: application/json

{
  "items": ["London", "Paris", "Berlin", "Tokyo", "Sydney"],
  "batchSize": 2,
  "includePagination": true
}
```

**Response:**
```json
{
  "results": [
    {
      "city": "London",
      "success": true,
      "weather": { /* weather data */ },
      "batchInfo": {
        "batchNumber": 1,
        "totalBatches": 3,
        "itemInBatch": 1
      }
    }
  ],
  "successCount": 4,
  "failureCount": 1,
  "batchInfo": {
    "batchSize": 2,
    "totalBatches": 3,
    "currentBatch": 3
  }
}
```

### 5. Pagination Demo
```http
POST /level4/paginate/demo
Content-Type: application/json

{
  "page": 2,
  "pageSize": 15,
  "sortBy": "name",
  "sortDirection": "Ascending"
}
```
*Demonstrates pagination with 157 sample items*

### 6. Pagination Info Helper
```http
GET /level4/pagination/info?totalItems=157&page=5&pageSize=20
```

**Response:**
```json
{
  "metadata": {
    "currentPage": 5,
    "pageSize": 20,
    "totalItems": 157,
    "totalPages": 8,
    "hasNextPage": true,
    "hasPreviousPage": true,
    "firstItemIndex": 81,
    "lastItemIndex": 100
  },
  "explanation": {
    "totalItems": 157,
    "requestedPage": 5,
    "requestedPageSize": 20,
    "validatedPage": 5,
    "validatedPageSize": 20,
    "calculatedSkip": 80
  }
}
```

### 7. Advanced Multi-Source Search
```http
POST /level4/search/advanced
Content-Type: application/json

{
  "query": "United",
  "page": 1,
  "pageSize": 20,
  "includeCities": true,
  "includeCountries": true,
  "countryFilter": null
}
```

## 🛠️ Architecture Details

### Pagination Models
```csharp
// Base pagination request
public record PaginationRequest(
    int Page = 1,
    int PageSize = 10,
    string? SortBy = null,
    SortDirection SortDirection = SortDirection.Ascending
);

// Rich pagination metadata
public record PaginationMetadata(
    int CurrentPage,
    int PageSize,
    int TotalItems,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage,
    int FirstItemIndex,
    int LastItemIndex
);

// Generic paginated response wrapper
public record PaginatedResponse<T>(
    IEnumerable<T> Data,
    PaginationMetadata Pagination
);
```

### Service Architecture
```csharp
public interface IPaginationService
{
    Task<PaginatedResponse<WeatherForecastRecord>> GetPaginatedWeatherHistoryAsync(
        WeatherHistoryPaginationRequest request);
    
    Task<PaginatedResponse<AutocompleteItem>> GetPaginatedAutocompleteAsync(
        AutocompletePaginationRequest request);
    
    Task<PaginatedResponse<AutocompleteItem>> GetPaginatedCitiesAsync(
        CitiesListRequest request);
    
    Task<BulkOperationResponse<object>> GetBulkWeatherWithPaginationAsync(
        BulkOperationRequest<string> request);
    
    PaginatedResponse<T> PaginateCollection<T>(
        IEnumerable<T> items, PaginationRequest request);
}
```

### Validation & Safety
- **Page validation**: Negative pages become 1, out-of-range pages handled gracefully
- **Size limits**: Page sizes clamped between 1-100 to prevent performance issues
- **Input sanitization**: All search queries and filters properly validated
- **Error resilience**: Bulk operations continue processing despite individual failures

## 📊 Performance Considerations

### Efficiency Features
- **Smart caching**: Reuses existing autocomplete cache for city data
- **Batch processing**: Configurable batch sizes prevent resource exhaustion
- **Lazy evaluation**: Pagination applied efficiently using LINQ
- **Memory management**: Large collections processed in chunks

### Best Practices
- **Reasonable page sizes**: Default to 10-20 items, max 100
- **Efficient sorting**: Pre-sorted data when possible
- **Filter early**: Apply filters before pagination for better performance
- **Batch sizing**: Use 5-25 items per batch for bulk operations

## 🧪 Testing

### Comprehensive Test Coverage
- **Unit tests**: `PaginationServiceTests.cs` - 12 tests covering core logic
- **Integration tests**: `Level4EndpointsTests.cs` - 15 tests covering end-to-end scenarios
- **Edge cases**: Empty collections, invalid inputs, boundary conditions
- **Performance tests**: Large collections, complex filtering scenarios

### Test Examples
```bash
# Run all Level 4 tests
dotnet test --filter "Level4"

# Run pagination service tests
dotnet test --filter "PaginationService"

# Run specific test
dotnet test --filter "PaginationDemo_ReturnsCorrectStructure"
```

## 💡 Usage Examples

### Frontend Integration

#### JavaScript/TypeScript
```typescript
// Fetch paginated cities
async function fetchCities(page: number = 1, country?: string) {
  const response = await fetch('/level4/cities/browse', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      page,
      pageSize: 25,
      sortBy: 'name',
      sortDirection: 'Ascending',
      countryFilter: country
    })
  });
  
  const result = await response.json();
  return {
    cities: result.data,
    pagination: result.pagination
  };
}

// Handle pagination navigation
function createPagination(pagination) {
  return {
    current: pagination.currentPage,
    total: pagination.totalPages,
    hasNext: pagination.hasNextPage,
    hasPrev: pagination.hasPreviousPage,
    itemRange: `${pagination.firstItemIndex}-${pagination.lastItemIndex} of ${pagination.totalItems}`
  };
}
```

#### React Hook Example
```tsx
function usePaginatedWeatherHistory(city: string) {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(false);
  
  const fetchPage = async (page: number, filters?: any) => {
    setLoading(true);
    try {
      const response = await fetch('/level4/weather/history/paginated', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ city, page, ...filters })
      });
      
      const result = await response.json();
      setData(result);
    } finally {
      setLoading(false);
    }
  };
  
  return { data, loading, fetchPage };
}
```

### CLI Examples
```bash
# Browse cities with pagination
curl -X GET "http://localhost:5051/level4/cities/browse?page=2&pageSize=5&sortBy=name"

# Advanced weather history search
curl -X POST "http://localhost:5051/level4/weather/history/paginated" \
  -H "Content-Type: application/json" \
  -d '{
    "city": "Berlin",
    "page": 1,
    "pageSize": 10,
    "sortBy": "temperature",
    "sortDirection": "Descending",
    "minTemperature": 15
  }'

# Bulk weather processing
curl -X POST "http://localhost:5051/level4/weather/bulk/paginated" \
  -H "Content-Type: application/json" \
  -d '{
    "items": ["London", "Paris", "Berlin", "Tokyo"],
    "batchSize": 2,
    "includePagination": true
  }'
```

## 🔗 Related Documentation

- [Level 1 - Basic Weather API](./README.md#level-1---external-weather-api)
- [Level 2 - Database CRUD](./README.md#level-2---database-crud-operations)  
- [Level 3 - Autocomplete Service](./API_REFERENCE.md)
- [Frontend Integration Guide](./FRONTEND_INTEGRATION.md)
- [API Quick Reference](./API_QUICK_REFERENCE.md)

## 🎯 Next Steps

Level 4 completes the core data management features. Future enhancements could include:

- **Level 5**: Real-time features (WebSockets, SignalR)
- **Level 6**: Advanced caching (Redis, distributed cache)
- **Level 7**: Authentication & authorization
- **Level 8**: Analytics & monitoring
- **Level 9**: Event-driven architecture
- **Level 10**: Microservices architecture 