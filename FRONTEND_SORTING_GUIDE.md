# Frontend Sorting Guide - Level 4 Pagination API

## 🎯 Overview

This guide provides frontend developers with everything needed to implement sorting functionality with our Level 4 pagination API. Our backend supports multi-criteria sorting across all paginated endpoints with consistent patterns and predictable behavior.

## 📋 Quick Reference

### Available Sort Fields by Endpoint

| Endpoint | Available Sort Fields | Default Sort |
|----------|----------------------|--------------|
| `/level4/weather/history/paginated` | `date`, `temperature`, `city`, `summary` | `date` (desc) |
| `/level4/weather-data/paginated` | `date`, `temperatureC`, `city`, `country`, `summary` | `date` (desc) |
| `/level4/autocomplete/paginated` | `relevance`, `name`, `country`, `population` | `relevance` |
| `/level4/cities/browse` | `name`, `country`, `population` | `name` (asc) |
| `/level4/search/advanced` | `name`, `country`, `population` | `relevance` |

### Sort Directions
- `"Ascending"` - A→Z, 0→9, oldest→newest
- `"Descending"` - Z→A, 9→0, newest→oldest

## 🔧 Implementation Examples

### 1. Basic Sorting Implementation

#### TypeScript Interface
```typescript
interface SortConfig {
  sortBy: string;
  sortDirection: 'Ascending' | 'Descending';
}

interface PaginationRequest {
  page: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: 'Ascending' | 'Descending';
}

interface SortOption {
  value: string;
  label: string;
  description?: string;
}
```

#### Sort Configuration Objects
```typescript
// Weather History Sorting Options
export const WEATHER_SORT_OPTIONS: SortOption[] = [
  { value: 'date', label: 'Date', description: 'Sort by forecast date' },
  { value: 'temperature', label: 'Temperature', description: 'Sort by temperature' },
  { value: 'city', label: 'City', description: 'Sort alphabetically by city' },
  { value: 'summary', label: 'Weather', description: 'Sort by weather description' }
];

// Weather Data Sorting Options (for /level4/weather-data/paginated)
export const WEATHER_DATA_SORT_OPTIONS: SortOption[] = [
  { value: 'date', label: 'Date', description: 'Sort by forecast date' },
  { value: 'temperatureC', label: 'Temperature', description: 'Sort by temperature (Celsius)' },
  { value: 'city', label: 'City', description: 'Sort alphabetically by city' },
  { value: 'country', label: 'Country', description: 'Sort by country name' },
  { value: 'summary', label: 'Weather', description: 'Sort by weather description' }
];

// Cities/Autocomplete Sorting Options
export const CITIES_SORT_OPTIONS: SortOption[] = [
  { value: 'relevance', label: 'Relevance', description: 'Best matches first' },
  { value: 'name', label: 'Name', description: 'Alphabetical order' },
  { value: 'country', label: 'Country', description: 'Sort by country name' },
  { value: 'population', label: 'Population', description: 'Sort by city size' }
];

// Default sort configurations
export const DEFAULT_SORTS = {
  weather: { sortBy: 'date', sortDirection: 'Descending' as const },
  weatherData: { sortBy: 'date', sortDirection: 'Descending' as const },
  cities: { sortBy: 'name', sortDirection: 'Ascending' as const },
  autocomplete: { sortBy: 'relevance', sortDirection: 'Descending' as const }
};
```

### 2. React Hook for Sorting

```tsx
import { useState, useCallback } from 'react';

interface UseSortingProps {
  defaultSort: SortConfig;
  onSortChange?: (sort: SortConfig) => void;
}

export function useSorting({ defaultSort, onSortChange }: UseSortingProps) {
  const [currentSort, setCurrentSort] = useState<SortConfig>(defaultSort);

  const updateSort = useCallback((newSortBy: string) => {
    setCurrentSort(prev => {
      const newSort: SortConfig = {
        sortBy: newSortBy,
        sortDirection: prev.sortBy === newSortBy 
          ? (prev.sortDirection === 'Ascending' ? 'Descending' : 'Ascending')
          : 'Ascending'
      };
      
      onSortChange?.(newSort);
      return newSort;
    });
  }, [onSortChange]);

  const setSortDirection = useCallback((direction: 'Ascending' | 'Descending') => {
    setCurrentSort(prev => {
      const newSort = { ...prev, sortDirection: direction };
      onSortChange?.(newSort);
      return newSort;
    });
  }, [onSortChange]);

  const resetSort = useCallback(() => {
    setCurrentSort(defaultSort);
    onSortChange?.(defaultSort);
  }, [defaultSort, onSortChange]);

  return {
    currentSort,
    updateSort,
    setSortDirection,
    resetSort,
    isAscending: currentSort.sortDirection === 'Ascending'
  };
}
```

### 3. Sort Component Examples

#### Simple Sort Dropdown
```tsx
interface SortDropdownProps {
  options: SortOption[];
  currentSort: SortConfig;
  onSortChange: (sortBy: string) => void;
  onDirectionChange: (direction: 'Ascending' | 'Descending') => void;
}

export function SortDropdown({ 
  options, 
  currentSort, 
  onSortChange, 
  onDirectionChange 
}: SortDropdownProps) {
  return (
    <div className="flex gap-2 items-center">
      <select 
        value={currentSort.sortBy}
        onChange={(e) => onSortChange(e.target.value)}
        className="px-3 py-2 border rounded-lg"
      >
        {options.map(option => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
      
      <button
        onClick={() => onDirectionChange(
          currentSort.sortDirection === 'Ascending' ? 'Descending' : 'Ascending'
        )}
        className="px-3 py-2 border rounded-lg hover:bg-gray-50"
        title={`Currently: ${currentSort.sortDirection}`}
      >
        {currentSort.sortDirection === 'Ascending' ? '↑' : '↓'}
      </button>
    </div>
  );
}
```

#### Advanced Sort Header
```tsx
interface SortableColumnProps {
  field: string;
  label: string;
  currentSort: SortConfig;
  onSort: (field: string) => void;
  className?: string;
}

export function SortableColumn({ 
  field, 
  label, 
  currentSort, 
  onSort, 
  className = '' 
}: SortableColumnProps) {
  const isActive = currentSort.sortBy === field;
  const isAscending = currentSort.sortDirection === 'Ascending';

  return (
    <button
      onClick={() => onSort(field)}
      className={`
        flex items-center gap-1 px-3 py-2 text-left hover:bg-gray-100 
        ${isActive ? 'font-semibold text-blue-600' : 'text-gray-700'}
        ${className}
      `}
    >
      {label}
      <span className={`text-xs transition-opacity ${isActive ? 'opacity-100' : 'opacity-0'}`}>
        {isAscending ? '↑' : '↓'}
      </span>
    </button>
  );
}
```

### 4. API Integration Examples

#### Weather History with Sorting
```typescript
interface WeatherHistoryFilters {
  city: string;
  minTemperature?: number;
  maxTemperature?: number;
  startDate?: string;
  endDate?: string;
}

async function fetchWeatherHistory(
  filters: WeatherHistoryFilters,
  pagination: PaginationRequest
) {
  const response = await fetch('/level4/weather/history/paginated', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      ...filters,
      page: pagination.page,
      pageSize: pagination.pageSize,
      sortBy: pagination.sortBy || 'date',
      sortDirection: pagination.sortDirection || 'Descending'
    })
  });

  if (!response.ok) {
    throw new Error(`Failed to fetch weather history: ${response.statusText}`);
  }

  return response.json();
}
```

#### Weather Data with Sorting (Real Data)
```typescript
interface WeatherDataFilters {
  city?: string;
  country?: string;
  summary?: string;
  dateFrom?: string;
  dateTo?: string;
  minTemperature?: number;
  maxTemperature?: number;
}

async function fetchWeatherData(
  filters: WeatherDataFilters,
  pagination: PaginationRequest
) {
  const response = await fetch('/level4/weather-data/paginated', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      ...filters,
      page: pagination.page,
      pageSize: pagination.pageSize,
      sortBy: pagination.sortBy || 'date',
      sortDirection: pagination.sortDirection || 'Descending'
    })
  });

  if (!response.ok) {
    throw new Error(`Failed to fetch weather data: ${response.statusText}`);
  }

  return response.json();
}
```

#### Cities Browse with Sorting
```typescript
interface CitiesFilters {
  countryFilter?: string;
  searchQuery?: string;
  minPopulation?: number;
  maxPopulation?: number;
}

async function fetchCities(
  filters: CitiesFilters,
  pagination: PaginationRequest
) {
  // For GET request with query params
  const params = new URLSearchParams({
    page: pagination.page.toString(),
    pageSize: pagination.pageSize.toString(),
    ...(pagination.sortBy && { sortBy: pagination.sortBy }),
    ...(pagination.sortDirection && { sortDirection: pagination.sortDirection }),
    ...(filters.countryFilter && { countryFilter: filters.countryFilter }),
    ...(filters.searchQuery && { searchQuery: filters.searchQuery }),
    ...(filters.minPopulation && { minPopulation: filters.minPopulation.toString() })
  });

  const response = await fetch(`/level4/cities/browse?${params}`);
  
  // OR for POST request with body
  const responsePost = await fetch('/level4/cities/browse', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      ...filters,
      ...pagination
    })
  });

  return response.json();
}
```

### 5. Complete React Component Example

```tsx
import React, { useState, useEffect } from 'react';

interface WeatherHistoryTableProps {
  city: string;
}

export function WeatherHistoryTable({ city }: WeatherHistoryTableProps) {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { currentSort, updateSort } = useSorting({
    defaultSort: DEFAULT_SORTS.weather,
    onSortChange: (newSort) => {
      // Refetch data when sort changes
      fetchData(1, newSort);
    }
  });

  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 10;

  const fetchData = async (page: number, sort?: SortConfig) => {
    setLoading(true);
    setError(null);
    
    try {
      const response = await fetchWeatherHistory(
        { city },
        { 
          page, 
          pageSize, 
          sortBy: sort?.sortBy || currentSort.sortBy,
          sortDirection: sort?.sortDirection || currentSort.sortDirection
        }
      );
      setData(response);
      setCurrentPage(page);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData(1);
  }, [city]);

  if (loading) return <div>Loading weather history...</div>;
  if (error) return <div className="text-red-600">Error: {error}</div>;
  if (!data) return null;

  return (
    <div className="space-y-4">
      {/* Sort Controls */}
      <div className="flex justify-between items-center">
        <h3 className="text-lg font-semibold">Weather History for {city}</h3>
        <SortDropdown
          options={WEATHER_SORT_OPTIONS}
          currentSort={currentSort}
          onSortChange={updateSort}
          onDirectionChange={(direction) => 
            setSortDirection(direction)
          }
        />
      </div>

      {/* Data Table */}
      <div className="overflow-x-auto">
        <table className="min-w-full border-collapse border border-gray-300">
          <thead>
            <tr className="bg-gray-50">
              <SortableColumn
                field="date"
                label="Date"
                currentSort={currentSort}
                onSort={updateSort}
                className="border border-gray-300 px-4 py-2"
              />
              <SortableColumn
                field="temperature"
                label="Temperature (°C)"
                currentSort={currentSort}
                onSort={updateSort}
                className="border border-gray-300 px-4 py-2"
              />
              <SortableColumn
                field="summary"
                label="Weather"
                currentSort={currentSort}
                onSort={updateSort}
                className="border border-gray-300 px-4 py-2"
              />
            </tr>
          </thead>
          <tbody>
            {data.data.map((item, index) => (
              <tr key={index} className="hover:bg-gray-50">
                <td className="border border-gray-300 px-4 py-2">
                  {new Date(item.date).toLocaleDateString()}
                </td>
                <td className="border border-gray-300 px-4 py-2">
                  {item.temperatureC}°C
                </td>
                <td className="border border-gray-300 px-4 py-2">
                  {item.summary}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Pagination Controls */}
      <div className="flex justify-between items-center">
        <div className="text-sm text-gray-600">
          Showing {data.pagination.firstItemIndex}-{data.pagination.lastItemIndex} of {data.pagination.totalItems} results
        </div>
        <div className="flex gap-2">
          <button
            disabled={!data.pagination.hasPreviousPage}
            onClick={() => fetchData(currentPage - 1)}
            className="px-3 py-1 border rounded disabled:opacity-50"
          >
            Previous
          </button>
          <span className="px-3 py-1">
            Page {data.pagination.currentPage} of {data.pagination.totalPages}
          </span>
          <button
            disabled={!data.pagination.hasNextPage}
            onClick={() => fetchData(currentPage + 1)}
            className="px-3 py-1 border rounded disabled:opacity-50"
          >
            Next
          </button>
        </div>
      </div>
    </div>
  );
}
```

## 🎨 UI/UX Best Practices

### Visual Indicators
- **Active sort column**: Different color/font weight
- **Sort direction**: Clear arrows (↑↓) or chevrons
- **Loading states**: Show sorting is in progress
- **Default states**: Indicate which field is sorted by default

### User Experience
- **Click to sort**: Single click changes sort field
- **Click again to reverse**: Second click on same field reverses direction
- **Clear visual feedback**: Users should know what's sorted and how
- **Preserve sort**: Maintain sort when paginating unless explicitly changed

### Performance Tips
- **Debounce sort changes**: Avoid rapid API calls
- **Loading indicators**: Show when fetching sorted data
- **Error handling**: Graceful fallbacks when sorting fails
- **Default fallbacks**: Always have a sensible default sort

## 🔧 Advanced Features

### Multi-Level Sorting
```typescript
// For future enhancement - secondary sort criteria
interface AdvancedSortConfig {
  primary: SortConfig;
  secondary?: SortConfig;
}

// Example: Sort by country, then by population within each country
const multiSort: AdvancedSortConfig = {
  primary: { sortBy: 'country', sortDirection: 'Ascending' },
  secondary: { sortBy: 'population', sortDirection: 'Descending' }
};
```

### Sort Persistence
```typescript
// Save sort preferences to localStorage
function saveSortPreference(endpoint: string, sort: SortConfig) {
  localStorage.setItem(`sort_${endpoint}`, JSON.stringify(sort));
}

function loadSortPreference(endpoint: string, defaultSort: SortConfig): SortConfig {
  const saved = localStorage.getItem(`sort_${endpoint}`);
  return saved ? JSON.parse(saved) : defaultSort;
}
```

### URL State Management
```typescript
// Sync sort with URL parameters
function updateUrlWithSort(sort: SortConfig) {
  const url = new URL(window.location.href);
  url.searchParams.set('sortBy', sort.sortBy);
  url.searchParams.set('sortDir', sort.sortDirection);
  window.history.pushState({}, '', url);
}

function getSortFromUrl(defaultSort: SortConfig): SortConfig {
  const params = new URLSearchParams(window.location.search);
  return {
    sortBy: params.get('sortBy') || defaultSort.sortBy,
    sortDirection: (params.get('sortDir') as 'Ascending' | 'Descending') || defaultSort.sortDirection
  };
}
```

## 📱 Mobile Considerations

### Responsive Sort Controls
```tsx
// Mobile-friendly sort selector
export function MobileSortControl({ options, currentSort, onSortChange }: SortDropdownProps) {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div className="relative">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="flex items-center gap-2 px-4 py-2 bg-white border rounded-lg shadow-sm w-full justify-between"
      >
        <span>Sort: {options.find(o => o.value === currentSort.sortBy)?.label}</span>
        <span>{currentSort.sortDirection === 'Ascending' ? '↑' : '↓'}</span>
      </button>
      
      {isOpen && (
        <div className="absolute top-full left-0 right-0 mt-1 bg-white border rounded-lg shadow-lg z-10">
          {options.map(option => (
            <button
              key={option.value}
              onClick={() => {
                onSortChange(option.value);
                setIsOpen(false);
              }}
              className="w-full text-left px-4 py-3 hover:bg-gray-50 first:rounded-t-lg last:rounded-b-lg"
            >
              <div className="font-medium">{option.label}</div>
              {option.description && (
                <div className="text-sm text-gray-600">{option.description}</div>
              )}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
```

## 🐛 Debugging Sorting Issues

### Common Frontend Issues

#### 1. **Wrong Endpoint or Missing Backend Sorting**
```typescript
// ❌ ISSUE: Some endpoints might not implement sorting
// This was the case with /level4/weather-data/paginated initially

// ✅ SOLUTION: Always verify the endpoint supports sorting
const testSorting = async () => {
  // Test both sort directions to verify sorting works
  const ascending = await fetch('/level4/weather-data/paginated', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      page: 1,
      pageSize: 3,
      sortBy: 'temperatureC',
      sortDirection: 'Ascending'
    })
  });

  const descending = await fetch('/level4/weather-data/paginated', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      page: 1,
      pageSize: 3,
      sortBy: 'temperatureC',
      sortDirection: 'Descending'
    })
  });

  const ascData = await ascending.json();
  const descData = await descending.json();

  // Compare first temperatures to verify sorting works
  const ascFirstTemp = ascData.data[0]?.temperatureC;
  const descFirstTemp = descData.data[0]?.temperatureC;

  console.log('Sorting verification:', {
    ascending: ascFirstTemp,
    descending: descFirstTemp,
    sortingWorks: ascFirstTemp !== descFirstTemp || ascFirstTemp < descFirstTemp
  });
};
```

#### 2. **Caching Problems**
```typescript
// Add cache-busting headers
const response = await fetch('/level4/weather/history/paginated', {
  method: 'POST',
  headers: { 
    'Content-Type': 'application/json',
    'Cache-Control': 'no-cache, no-store, must-revalidate'
  },
  body: JSON.stringify(request)
});
```

#### 3. **State Management Issues**
```typescript
// Debug your sort requests
const debugFetch = async (request: PaginationRequest, endpoint = '/level4/weather-data/paginated') => {
  console.log('🔍 Sorting Debug:', {
    sortBy: request.sortBy,
    sortDirection: request.sortDirection,
    endpoint,
    timestamp: new Date().toISOString()
  });
  
  const response = await fetch(endpoint, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request)
  });
  
  const data = await response.json();
  
  console.log('📊 First 5 temperatures:', 
    data.data.slice(0, 5).map(item => ({
      temp: item.temperatureC,
      city: item.city,
      date: item.date
    }))
  );
  
  return data;
};
```

#### 4. **Race Condition Prevention**
```typescript
// Debounce sort changes to prevent rapid API calls
import { debounce } from 'lodash';

const debouncedSort = debounce(async (sortConfig: SortConfig) => {
  setLoading(true);
  try {
    const data = await fetchWithSort(sortConfig);
    setResults(data);
  } finally {
    setLoading(false);
  }
}, 300); // Wait 300ms after last sort change
```

### Verification Checklist

#### ✅ **Backend Verification**
```bash
# Test weather history endpoint - ascending sort
curl -X POST "http://localhost:5249/level4/weather/history/paginated" \
  -H "Content-Type: application/json" \
  -d '{"city": "London", "page": 1, "pageSize": 3, "sortBy": "temperatureC", "sortDirection": "Ascending"}'

# Test weather history endpoint - descending sort  
curl -X POST "http://localhost:5249/level4/weather/history/paginated" \
  -H "Content-Type: application/json" \
  -d '{"city": "London", "page": 1, "pageSize": 3, "sortBy": "temperatureC", "sortDirection": "Descending"}'

# Test weather data endpoint (real data) - ascending sort
curl -X POST "http://localhost:5249/level4/weather-data/paginated" \
  -H "Content-Type: application/json" \
  -d '{"page": 1, "pageSize": 3, "sortBy": "temperatureC", "sortDirection": "Ascending"}'

# Test weather data endpoint (real data) - descending sort
curl -X POST "http://localhost:5249/level4/weather-data/paginated" \
  -H "Content-Type: application/json" \
  -d '{"page": 1, "pageSize": 3, "sortBy": "temperatureC", "sortDirection": "Descending"}'
```

#### ✅ **Frontend Network Tab**
1. Open Developer Tools → Network Tab
2. Trigger a sort change
3. Check the request payload:
   ```json
   {
     "city": "London",
     "sortBy": "temperatureC",
     "sortDirection": "Ascending",
     "page": 1,
     "pageSize": 10
   }
   ```
4. Verify the response data shows correct sorting

#### ✅ **React DevTools**
- Check if sort state is updating correctly
- Verify no stale state is being used
- Confirm re-renders are happening on sort changes

## 🚀 Quick Start Checklist

1. **Choose your sorting approach**:
   - [ ] Simple dropdown for basic needs
   - [ ] Column headers for table data
   - [ ] Advanced multi-criteria for complex data

2. **Implement sort state management**:
   - [ ] Use the `useSorting` hook or similar state management
   - [ ] Set appropriate default sorts for each endpoint
   - [ ] Handle sort changes with API calls

3. **Add visual indicators**:
   - [ ] Show current sort field and direction
   - [ ] Add hover states and clear interactions
   - [ ] Include loading states during sort changes

4. **Add debugging (during development)**:
   - [ ] Log all sort requests and responses
   - [ ] Verify correct parameters are sent to API
   - [ ] Check for caching or race conditions
   - [ ] Test with browser network tab

5. **Test thoroughly**:
   - [ ] Verify all sort fields work correctly
   - [ ] Test sort direction toggles
   - [ ] Check pagination works with sorting
   - [ ] Validate mobile responsiveness

## 🔗 Related Documentation

- [Level 4 Pagination Guide](./LEVEL4_PAGINATION_GUIDE.md)
- [Frontend Integration Guide](./FRONTEND_INTEGRATION.md)
- [API Quick Reference](./API_QUICK_REFERENCE.md)
- [Frontend Pagination Integration](./FRONTEND_PAGINATION_INTEGRATION.md)

---

**Need Help?** Check our [API examples](./INTEGRATION_EXAMPLES.md) or test the [live API demo](./LIVE_API_DEMO.md) to see sorting in action! 