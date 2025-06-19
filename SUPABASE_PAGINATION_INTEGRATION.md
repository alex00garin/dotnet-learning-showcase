# 🚀 Supabase Integration Guide - Level 4 Pagination

## Your pagination system works PERFECTLY with Supabase PostgreSQL tables

---

## 🎯 Why It Works Better with Supabase

✅ **Database-Level Pagination**: Uses OFFSET/LIMIT directly in SQL  
✅ **Efficient Queries**: Only fetches needed data, not entire tables  
✅ **Real Counting**: Accurate total counts from PostgreSQL  
✅ **Advanced Filtering**: Leverages PostgreSQL WHERE clauses  
✅ **Proper Sorting**: Database-optimized ORDER BY clauses  

**Your existing Level 4 pagination will work even better with real database performance!**

---

## 🔧 Backend Integration with Supabase

### 1. Install Supabase Package

```bash
dotnet add package Supabase
```

### 2. Supabase Service Implementation

```csharp
// Services/SupabaseService.cs
using Supabase;
using Supabase.Postgrest;

public interface ISupabaseService
{
    Task<PaginatedResponse<T>> GetPaginatedAsync<T>(
        string tableName,
        PaginationRequest request,
        Dictionary<string, object>? filters = null
    ) where T : BaseModel, new();
}

public class SupabaseService : ISupabaseService
{
    private readonly Supabase.Client _supabase;

    public SupabaseService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public async Task<PaginatedResponse<T>> GetPaginatedAsync<T>(
        string tableName,
        PaginationRequest request,
        Dictionary<string, object>? filters = null
    ) where T : BaseModel, new()
    {
        var pageSize = Math.Min(request.PageSize ?? 10, 100);
        var page = Math.Max(request.Page ?? 1, 1);
        var offset = (page - 1) * pageSize;

        // Build query with filters
        var query = _supabase.From<T>();

        if (filters != null)
        {
            foreach (var (key, value) in filters)
            {
                if (value is string str && !string.IsNullOrEmpty(str))
                    query = query.Ilike(key, $"%{str}%");
                else if (value != null)
                    query = query.Eq(key, value);
            }
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(request.SortBy))
        {
            var ascending = request.SortDirection == "Ascending";
            query = query.Order(request.SortBy, ascending 
                ? Postgrest.Constants.Ordering.Asc 
                : Postgrest.Constants.Ordering.Desc);
        }

        // Get total count
        var countResponse = await query.Select("*", Postgrest.Constants.CountType.Exact).Get();
        var totalItems = countResponse.Count ?? 0;

        // Get paginated data
        var dataResponse = await query
            .Range(offset, offset + pageSize - 1)
            .Get();

        // Build response using your existing helper
        return PaginationHelper.CreateResponse(
            dataResponse.Models,
            page,
            pageSize,
            totalItems
        );
    }
}
```

### 3. Supabase Models

```csharp
// Models/SupabaseModels.cs
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("weather_records")]
public class WeatherRecord : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("city")]
    public string City { get; set; } = string.Empty;

    [Column("country")]
    public string Country { get; set; } = string.Empty;

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("temperature_c")]
    public double TemperatureC { get; set; }

    [Column("humidity")]
    public int Humidity { get; set; }

    [Column("summary")]
    public string Summary { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}

[Table("cities")]
public class City : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("country_code")]
    public string CountryCode { get; set; } = string.Empty;

    [Column("population")]
    public long? Population { get; set; }

    [Column("latitude")]
    public double Latitude { get; set; }

    [Column("longitude")]
    public double Longitude { get; set; }
}
```

### 4. Enhanced Level 4 Endpoints for Supabase

```csharp
// Endpoints/Level4SupabaseEndpoints.cs
public static class Level4SupabaseEndpoints
{
    public static void MapLevel4SupabaseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/level4/supabase")
            .WithTags("Level 4 - Supabase Pagination");

        // Weather records from Supabase
        group.MapPost("/weather", async (
            WeatherPaginationRequest request,
            ISupabaseService supabase) =>
        {
            var filters = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(request.City)) filters["city"] = request.City;
            if (!string.IsNullOrEmpty(request.Country)) filters["country"] = request.Country;

            var result = await supabase.GetPaginatedAsync<WeatherRecord>(
                "weather_records", request, filters);

            return Results.Ok(result);
        });

        // Cities from Supabase  
        group.MapPost("/cities", async (
            CitiesPaginationRequest request,
            ISupabaseService supabase) =>
        {
            var filters = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(request.SearchQuery)) filters["name"] = request.SearchQuery;
            if (!string.IsNullOrEmpty(request.CountryCode)) filters["country_code"] = request.CountryCode;

            var result = await supabase.GetPaginatedAsync<City>(
                "cities", request, filters);

            return Results.Ok(result);
        });
    }
}

public record WeatherPaginationRequest : PaginationRequest
{
    public string? City { get; init; }
    public string? Country { get; init; }
    public DateTime? DateFrom { get; init; }
    public double? MinTemperature { get; init; }
}

public record CitiesPaginationRequest : PaginationRequest  
{
    public string? SearchQuery { get; init; }
    public string? CountryCode { get; init; }
    public long? MinPopulation { get; init; }
}
```

### 5. Program.cs Configuration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add Supabase
builder.Services.AddScoped<Supabase.Client>(_ => 
    new Supabase.Client(
        builder.Configuration["Supabase:Url"]!,
        builder.Configuration["Supabase:AnonKey"]!,
        new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = false
        }));

builder.Services.AddScoped<ISupabaseService, SupabaseService>();

var app = builder.Build();

// Map your existing Level 4 endpoints AND new Supabase endpoints
app.MapLevel4Endpoints();
app.MapLevel4SupabaseEndpoints();
```

```json
// appsettings.json
{
  "Supabase": {
    "Url": "https://your-project.supabase.co",
    "AnonKey": "your-anon-key"
  }
}
```

---

## 🗄️ Supabase Database Setup

### SQL Schema

```sql
-- Weather records table
CREATE TABLE weather_records (
    id SERIAL PRIMARY KEY,
    city VARCHAR(100) NOT NULL,
    country VARCHAR(100) NOT NULL,
    date DATE NOT NULL,
    temperature_c DECIMAL(5,2) NOT NULL,
    humidity INTEGER CHECK (humidity >= 0 AND humidity <= 100),
    summary VARCHAR(200),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Cities table  
CREATE TABLE cities (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    country_code VARCHAR(3) NOT NULL,
    population BIGINT,
    latitude DECIMAL(10,8) NOT NULL,
    longitude DECIMAL(11,8) NOT NULL
);

-- Performance indexes
CREATE INDEX idx_weather_city ON weather_records(city);
CREATE INDEX idx_weather_date ON weather_records(date);
CREATE INDEX idx_cities_name ON cities(name);
CREATE INDEX idx_cities_country ON cities(country_code);

-- Sample data
INSERT INTO weather_records (city, country, date, temperature_c, humidity, summary)
VALUES 
    ('London', 'UK', '2024-01-15', 12.5, 75, 'Cloudy'),
    ('Paris', 'France', '2024-01-15', 8.2, 82, 'Rainy'),
    ('Berlin', 'Germany', '2024-01-15', 5.1, 68, 'Sunny');

INSERT INTO cities (name, country_code, population, latitude, longitude)
VALUES 
    ('London', 'GB', 8982000, 51.5074, -0.1278),
    ('Paris', 'FR', 2161000, 48.8566, 2.3522),
    ('Berlin', 'DE', 3669000, 52.5200, 13.4050);
```

---

## 🌐 Frontend Integration (Direct Supabase)

### React Hook for Direct Supabase Access

```typescript
// hooks/useSupabasePagination.ts
import { useState, useEffect } from 'react';
import { createClient } from '@supabase/supabase-js';

const supabase = createClient(
  process.env.REACT_APP_SUPABASE_URL!,
  process.env.REACT_APP_SUPABASE_ANON_KEY!
);

export function useSupabasePagination<T>(
  tableName: string,
  filters: Record<string, any> = {},
  initialPageSize = 10
) {
  const [data, setData] = useState<T[]>([]);
  const [pagination, setPagination] = useState<PaginationMetadata | null>(null);
  const [loading, setLoading] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(initialPageSize);
  const [sortBy, setSortBy] = useState('');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');

  const loadData = async () => {
    setLoading(true);
    
    try {
      const from = (currentPage - 1) * pageSize;
      const to = from + pageSize - 1;

      let query = supabase
        .from(tableName)
        .select('*', { count: 'exact' });

      // Apply filters
      Object.entries(filters).forEach(([key, value]) => {
        if (value) {
          if (typeof value === 'string') {
            query = query.ilike(key, `%${value}%`);
          } else {
            query = query.eq(key, value);
          }
        }
      });

      // Apply sorting
      if (sortBy) {
        query = query.order(sortBy, { ascending: sortDirection === 'asc' });
      }

      // Apply pagination
      const { data: results, count, error } = await query.range(from, to);

      if (error) throw error;

      const totalItems = count || 0;
      const totalPages = Math.ceil(totalItems / pageSize);

      setData(results || []);
      setPagination({
        currentPage,
        pageSize,
        totalItems,
        totalPages,
        hasNextPage: currentPage < totalPages,
        hasPreviousPage: currentPage > 1,
        firstItemIndex: totalItems > 0 ? from + 1 : 0,
        lastItemIndex: Math.min(to + 1, totalItems)
      });

    } catch (error) {
      console.error('Pagination error:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [currentPage, pageSize, sortBy, sortDirection, JSON.stringify(filters)]);

  return {
    data,
    pagination,
    loading,
    currentPage,
    pageSize,
    sortBy,
    sortDirection,
    setCurrentPage,
    setPageSize,
    setSortBy,
    setSortDirection,
    refresh: loadData
  };
}
```

### React Component Example

```tsx
// components/SupabaseWeatherTable.tsx
import React, { useState } from 'react';
import { useSupabasePagination } from '../hooks/useSupabasePagination';

interface WeatherRecord {
  id: number;
  city: string;
  country: string;
  date: string;
  temperature_c: number;
  humidity: number;
  summary: string;
}

export const SupabaseWeatherTable: React.FC = () => {
  const [cityFilter, setCityFilter] = useState('');
  const [countryFilter, setCountryFilter] = useState('');

  const {
    data,
    pagination,
    loading,
    currentPage,
    pageSize,
    setCurrentPage,
    setPageSize,
    setSortBy,
    setSortDirection
  } = useSupabasePagination<WeatherRecord>('weather_records', {
    city: cityFilter,
    country: countryFilter
  });

  if (loading) return <div>Loading from Supabase...</div>;

  return (
    <div>
      {/* Filters */}
      <div className="filters">
        <input
          placeholder="Filter by city..."
          value={cityFilter}
          onChange={(e) => setCityFilter(e.target.value)}
        />
        <input
          placeholder="Filter by country..."
          value={countryFilter}
          onChange={(e) => setCountryFilter(e.target.value)}
        />
      </div>

      {/* Controls */}
      <div className="controls">
        <select value={pageSize} onChange={(e) => setPageSize(Number(e.target.value))}>
          <option value={10}>10 per page</option>
          <option value={25}>25 per page</option>
          <option value={50}>50 per page</option>
        </select>

        <select onChange={(e) => setSortBy(e.target.value)}>
          <option value="">No sorting</option>
          <option value="city">Sort by City</option>
          <option value="date">Sort by Date</option>
          <option value="temperature_c">Sort by Temperature</option>
        </select>
      </div>

      {/* Table */}
      <table>
        <thead>
          <tr>
            <th>City</th>
            <th>Country</th>
            <th>Date</th>
            <th>Temperature</th>
            <th>Humidity</th>
            <th>Summary</th>
          </tr>
        </thead>
        <tbody>
          {data.map((record) => (
            <tr key={record.id}>
              <td>{record.city}</td>
              <td>{record.country}</td>
              <td>{new Date(record.date).toLocaleDateString()}</td>
              <td>{record.temperature_c}°C</td>
              <td>{record.humidity}%</td>
              <td>{record.summary}</td>
            </tr>
          ))}
        </tbody>
      </table>

      {/* Pagination */}
      {pagination && (
        <div className="pagination">
          <span>
            Showing {pagination.firstItemIndex}-{pagination.lastItemIndex} of{' '}
            {pagination.totalItems} records
          </span>
          
          <button 
            onClick={() => setCurrentPage(currentPage - 1)}
            disabled={!pagination.hasPreviousPage}
          >
            Previous
          </button>
          
          <span>Page {currentPage} of {pagination.totalPages}</span>
          
          <button 
            onClick={() => setCurrentPage(currentPage + 1)}
            disabled={!pagination.hasNextPage}
          >
            Next
          </button>
        </div>
      )}
    </div>
  );
};
```

---

## 📊 Performance Benefits

### Database-Level Efficiency

```sql
-- Your pagination generates efficient SQL:

-- ✅ GOOD: Only fetch needed data
SELECT * FROM weather_records 
WHERE city ILIKE '%London%' 
ORDER BY date DESC 
LIMIT 10 OFFSET 20;

-- ❌ BAD: Loading all data in memory
SELECT * FROM weather_records WHERE city ILIKE '%London%';
```

### PostgreSQL Optimizations

```sql
-- Add performance indexes
CREATE INDEX CONCURRENTLY idx_weather_city_date ON weather_records(city, date);
CREATE INDEX CONCURRENTLY idx_cities_name_trgm ON cities USING gin(name gin_trgm_ops);

-- Monitor query performance
EXPLAIN (ANALYZE, BUFFERS) 
SELECT * FROM weather_records 
WHERE city ILIKE '%London%' 
ORDER BY date DESC 
LIMIT 10 OFFSET 0;
```

---

## 🧪 Testing Your Supabase Integration

### API Testing

```bash
# Test your new Supabase endpoints
curl -X POST http://localhost:5249/level4/supabase/weather \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 5,
    "city": "London",
    "sortBy": "date",
    "sortDirection": "Descending"
  }' | jq '.'

curl -X POST http://localhost:5249/level4/supabase/cities \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 10,
    "searchQuery": "Lon",
    "sortBy": "population",
    "sortDirection": "Descending"
  }' | jq '.'
```

### Response Format (Same as before!)

```json
{
  "data": [
    {
      "id": 1,
      "city": "London",
      "country": "UK",
      "date": "2024-01-15",
      "temperatureC": 12.5,
      "humidity": 75,
      "summary": "Cloudy"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 5,
    "totalItems": 150,
    "totalPages": 30,
    "hasNextPage": true,
    "hasPreviousPage": false,
    "firstItemIndex": 1,
    "lastItemIndex": 5
  }
}
```

---

## 🎯 Migration Strategy

### Phase 1: Add Supabase alongside existing system
```csharp
// Keep both your demo endpoints AND add Supabase endpoints
app.MapLevel4Endpoints();        // Your existing demo system
app.MapLevel4SupabaseEndpoints(); // New Supabase-powered endpoints
```

### Phase 2: Test with real data
- Create tables in Supabase
- Insert sample data
- Test all pagination features
- Compare performance

### Phase 3: Frontend chooses data source
```typescript
// Frontend can switch between demo and real data
const dataSource = useDemo ? '/level4/paginate/demo' : '/level4/supabase/weather';
```

---

## ✅ Quick Setup Checklist

- [ ] **Create Supabase project** at [supabase.com](https://supabase.com)
- [ ] **Get API URL and anon key** from project settings
- [ ] **Install `Supabase` NuGet package**
- [ ] **Create database tables** with the SQL above
- [ ] **Add configuration** to appsettings.json
- [ ] **Register services** in Program.cs
- [ ] **Create new endpoints** alongside existing ones
- [ ] **Test with curl** or frontend
- [ ] **Monitor performance** in Supabase dashboard

---

**🚀 Your Level 4 pagination system will work seamlessly with Supabase and provide enterprise-grade performance with real PostgreSQL database capabilities! The same frontend code works with both demo data and Supabase data.** 