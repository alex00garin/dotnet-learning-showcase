# 🚀 Frontend Integration Guide - Level 4 Pagination

## Complete guide for integrating Level 4 pagination endpoints with any frontend framework

---

## 📋 API Overview

### Base URLs
```
Production:  https://api.alexandergarin.com/level4
Development: http://localhost:5249/level4
```

### Universal Response Format
All paginated endpoints return this consistent structure:

```typescript
interface PaginatedResponse<T> {
  data: T[];
  pagination: {
    currentPage: number;
    pageSize: number;
    totalItems: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
    firstItemIndex: number;
    lastItemIndex: number;
  };
}
```

---

## 🎯 Available Endpoints

### 1. Real Weather Data Pagination
```http
POST /level4/weather-data/paginated
Content-Type: application/json

{
  "city": "London",
  "page": 1,
  "pageSize": 10,
  "sortBy": "date",
  "sortDirection": "Descending"
}
```

### 2. Cities Autocomplete with Pagination
```http
POST /level4/autocomplete/paginated
Content-Type: application/json

{
  "query": "New",
  "dataSource": "cities",
  "page": 1,
  "pageSize": 5,
  "sortBy": "name",
  "sortDirection": "Ascending",
  "countryFilter": "US",
  "minPopulation": 50000
}
```

### 3. Cities Browse (GET)
```http
GET /level4/cities/browse?page=1&pageSize=10&searchQuery=London&sortBy=name
```

### 4. Cities Browse (POST with filters)
```http
POST /level4/cities/browse
Content-Type: application/json

{
  "page": 1,
  "pageSize": 20,
  "sortBy": "name",
  "sortDirection": "Ascending",
  "searchQuery": "San",
  "countryFilter": "US",
  "minPopulation": 100000,
  "maxPopulation": 5000000
}
```

### 5. Weather History Pagination
```http
POST /level4/weather/history/paginated
Content-Type: application/json

{
  "city": "London",
  "page": 1,
  "pageSize": 10,
  "sortBy": "date",
  "sortDirection": "Descending",
  "dateFrom": "2024-01-01",
  "dateTo": "2024-12-31",
  "minTemperature": -10,
  "maxTemperature": 40
}
```

### 6. Bulk Weather Operations
```http
POST /level4/weather/bulk/paginated
Content-Type: application/json

{
  "items": ["London", "Paris", "Berlin"],
  "batchSize": 2,
  "includePagination": true
}
```

---

## 💻 TypeScript Interfaces

```typescript
// Core pagination types
interface PaginationRequest {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'Ascending' | 'Descending';
}

interface PaginationMetadata {
  currentPage: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  firstItemIndex: number;
  lastItemIndex: number;
}

interface PaginatedResponse<T> {
  data: T[];
  pagination: PaginationMetadata;
}

// Specific request types
interface AutocompletePaginationRequest extends PaginationRequest {
  query: string;
  dataSource?: string;
  countryFilter?: string;
  minPopulation?: number;
}

interface CitiesListRequest extends PaginationRequest {
  searchQuery?: string;
  countryFilter?: string;
  minPopulation?: number;
  maxPopulation?: number;
}

interface WeatherHistoryPaginationRequest extends PaginationRequest {
  city: string;
  dateFrom?: string;
  dateTo?: string;
  minTemperature?: number;
  maxTemperature?: number;
}

// Response data types
interface WeatherRecord {
  id: string;
  city: string;
  country: string;
  date: string;
  temperatureC: number;
  summary: string;
}

interface AutocompleteItem {
  id: string;
  label: string;
  value: string;
  metadata?: Record<string, any>;
}

interface CityItem {
  id: string;
  name: string;
  country: string;
  population?: number;
  latitude?: number;
  longitude?: number;
}
```

---

## ⚡ React Integration Examples

### Basic Pagination Hook
```typescript
import { useState, useEffect } from 'react';

interface UsePaginationProps<T> {
  fetchData: (request: PaginationRequest) => Promise<PaginatedResponse<T>>;
  initialPage?: number;
  initialPageSize?: number;
}

export function usePagination<T>({
  fetchData,
  initialPage = 1,
  initialPageSize = 10
}: UsePaginationProps<T>) {
  const [data, setData] = useState<T[]>([]);
  const [pagination, setPagination] = useState<PaginationMetadata | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const [currentPage, setCurrentPage] = useState(initialPage);
  const [pageSize, setPageSize] = useState(initialPageSize);
  const [sortBy, setSortBy] = useState<string>('');
  const [sortDirection, setSortDirection] = useState<'Ascending' | 'Descending'>('Ascending');

  const loadData = async () => {
    setLoading(true);
    setError(null);
    
    try {
      const request: PaginationRequest = {
        page: currentPage,
        pageSize,
        sortBy: sortBy || undefined,
        sortDirection
      };
      
      const response = await fetchData(request);
      setData(response.data);
      setPagination(response.pagination);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [currentPage, pageSize, sortBy, sortDirection]);

  return {
    // Data
    data,
    pagination,
    loading,
    error,
    
    // Controls
    currentPage,
    pageSize,
    sortBy,
    sortDirection,
    
    // Actions
    setCurrentPage,
    setPageSize,
    setSortBy,
    setSortDirection,
    refresh: loadData,
    
    // Convenience methods
    goToFirstPage: () => setCurrentPage(1),
    goToLastPage: () => pagination && setCurrentPage(pagination.totalPages),
    goToNextPage: () => pagination?.hasNextPage && setCurrentPage(currentPage + 1),
    goToPreviousPage: () => pagination?.hasPreviousPage && setCurrentPage(currentPage - 1)
  };
}
```

### Weather Data Component
```tsx
import React from 'react';
import { usePagination } from './usePagination';

// API Configuration
const API_BASE_URL = process.env.NODE_ENV === 'production' 
  ? 'https://api.alexandergarin.com/level4'
  : 'http://localhost:5249/level4';

const fetchWeatherData = async (request: WeatherHistoryPaginationRequest): Promise<PaginatedResponse<WeatherRecord>> => {
  const response = await fetch(`${API_BASE_URL}/weather-data/paginated`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request)
  });
  
  if (!response.ok) {
    throw new Error('Failed to fetch data');
  }
  
  return response.json();
};

export const WeatherDataTable: React.FC = () => {
  const {
    data,
    pagination,
    loading,
    error,
    currentPage,
    pageSize,
    sortBy,
    sortDirection,
    setCurrentPage,
    setPageSize,
    setSortBy,
    setSortDirection,
    goToFirstPage,
    goToLastPage,
    goToNextPage,
    goToPreviousPage
  } = usePagination({
    fetchData: (request) => fetchWeatherData({ 
      city: "London", 
      ...request 
    }),
    initialPageSize: 10
  });

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div className="weather-data-table">
      {/* Controls */}
      <div className="controls">
        <select 
          value={pageSize} 
          onChange={(e) => setPageSize(Number(e.target.value))}
        >
          <option value={5}>5 per page</option>
          <option value={10}>10 per page</option>
          <option value={25}>25 per page</option>
          <option value={50}>50 per page</option>
        </select>
        
        <select 
          value={sortBy} 
          onChange={(e) => setSortBy(e.target.value)}
        >
          <option value="">No sorting</option>
          <option value="city">Sort by City</option>
          <option value="country">Sort by Country</option>
          <option value="date">Sort by Date</option>
          <option value="temperatureC">Sort by Temperature</option>
          <option value="summary">Sort by Summary</option>
        </select>
        
        <button 
          onClick={() => setSortDirection(sortDirection === 'Ascending' ? 'Descending' : 'Ascending')}
        >
          {sortDirection} ↕️
        </button>
      </div>

      {/* Data Table */}
      <table>
        <thead>
          <tr>
            <th>ID</th>
            <th>City</th>
            <th>Country</th>
            <th>Temperature (°C)</th>
            <th>Summary</th>
            <th>Date</th>
          </tr>
        </thead>
        <tbody>
          {data.map((item) => (
            <tr key={item.id}>
              <td>{item.id.substring(0, 8)}...</td>
              <td>{item.city}</td>
              <td>{item.country}</td>
              <td>{item.temperatureC}°C</td>
              <td>{item.summary}</td>
              <td>{new Date(item.date).toLocaleDateString()}</td>
            </tr>
          ))}
        </tbody>
      </table>

      {/* Pagination Controls */}
      {pagination && (
        <div className="pagination-controls">
          <div className="pagination-info">
            Showing {pagination.firstItemIndex}-{pagination.lastItemIndex} of {pagination.totalItems} items
          </div>
          
          <div className="pagination-buttons">
            <button 
              onClick={goToFirstPage} 
              disabled={!pagination.hasPreviousPage}
            >
              First
            </button>
            <button 
              onClick={goToPreviousPage} 
              disabled={!pagination.hasPreviousPage}
            >
              Previous
            </button>
            
            <span>
              Page {pagination.currentPage} of {pagination.totalPages}
            </span>
            
            <button 
              onClick={goToNextPage} 
              disabled={!pagination.hasNextPage}
            >
              Next
            </button>
            <button 
              onClick={goToLastPage} 
              disabled={!pagination.hasNextPage}
            >
              Last
            </button>
          </div>
        </div>
      )}
    </div>
  );
};
```

---

## 🎨 CSS Styling Examples

### Modern Pagination Styles
```css
.pagination-controls {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  margin: 1rem 0;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 8px;
}

.pagination-info {
  font-size: 0.9rem;
  color: #6c757d;
}

.pagination-buttons {
  display: flex;
  gap: 0.5rem;
}

.pagination-buttons button {
  padding: 0.5rem 1rem;
  border: 1px solid #dee2e6;
  background: white;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.2s;
}

.pagination-buttons button:hover:not(:disabled) {
  background: #e9ecef;
  border-color: #adb5bd;
}

.pagination-buttons button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Table styles */
.data-table {
  width: 100%;
  border-collapse: collapse;
  margin: 1rem 0;
}

.data-table th,
.data-table td {
  padding: 0.75rem;
  text-align: left;
  border-bottom: 1px solid #dee2e6;
}

.data-table th {
  background: #f8f9fa;
  font-weight: 600;
}

.data-table tbody tr:hover {
  background: #f8f9fa;
}

/* Controls */
.controls {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
  flex-wrap: wrap;
}

.controls select,
.controls input {
  padding: 0.5rem;
  border: 1px solid #ced4da;
  border-radius: 4px;
  font-size: 0.9rem;
}

/* Loading and error states */
.loading {
  text-align: center;
  padding: 2rem;
  color: #6c757d;
}

.error {
  padding: 1rem;
  background: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
  border-radius: 4px;
  margin: 1rem 0;
}

/* Responsive design */
@media (max-width: 768px) {
  .pagination-controls {
    flex-direction: column;
    text-align: center;
  }
  
  .controls {
    flex-direction: column;
  }
  
  .data-table {
    font-size: 0.8rem;
  }
  
  .data-table th,
  .data-table td {
    padding: 0.5rem;
  }
}
```

---

## 🚀 Quick Start Examples

### Simple Fetch Function
```javascript
// Basic JavaScript example
async function fetchPaginatedData(endpoint, params = {}) {
  const defaultParams = {
    page: 1,
    pageSize: 10,
    sortDirection: 'Ascending'
  };
  
  const requestBody = { ...defaultParams, ...params };
  
  const response = await fetch(`/level4${endpoint}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(requestBody)
  });
  
  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }
  
  return response.json();
}

// Usage examples
fetchPaginatedData('/weather-data/paginated', { city: "London", page: 1, pageSize: 5 })
  .then(result => {
    console.log('Data:', result.data);
    console.log('Pagination:', result.pagination);
  });

fetchPaginatedData('/cities/browse', { 
  searchQuery: 'London', 
  page: 1, 
  pageSize: 10 
})
  .then(result => {
    console.log('Cities found:', result.data.length);
    console.log('Total pages:', result.pagination.totalPages);
  });
```

### Vanilla HTML + JavaScript Demo
```html
<!DOCTYPE html>
<html>
<head>
    <title>Pagination Demo</title>
    <style>
        .container { max-width: 800px; margin: 0 auto; padding: 20px; }
        .controls { margin-bottom: 20px; }
        .controls select, .controls input { margin: 5px; padding: 5px; }
        table { width: 100%; border-collapse: collapse; }
        th, td { padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }
        .pagination { margin-top: 20px; text-align: center; }
        .pagination button { margin: 5px; padding: 5px 10px; }
        .loading { text-align: center; padding: 20px; color: #666; }
    </style>
</head>
<body>
    <div class="container">
        <h1>Level 4 Weather Data Pagination</h1>
        
        <div class="controls">
            <select id="pageSize">
                <option value="5">5 per page</option>
                <option value="10" selected>10 per page</option>
                <option value="25">25 per page</option>
            </select>
            
            <select id="sortBy">
                <option value="">No sorting</option>
                <option value="city">Sort by City</option>
                <option value="country">Sort by Country</option>
                <option value="date">Sort by Date</option>
                <option value="temperatureC">Sort by Temperature</option>
                <option value="summary">Sort by Summary</option>
            </select>
            
            <button id="sortDirection">Ascending ↕️</button>
        </div>
        
        <div id="loading" class="loading" style="display: none;">Loading...</div>
        <div id="error" style="color: red; display: none;"></div>
        
        <table id="dataTable" style="display: none;">
            <thead>
                <tr>
                    <th>ID</th>
                    <th>City</th>
                    <th>Country</th>
                    <th>Temperature (°C)</th>
                    <th>Summary</th>
                    <th>Date</th>
                </tr>
            </thead>
            <tbody id="tableBody"></tbody>
        </table>
        
        <div id="pagination" class="pagination" style="display: none;">
            <button id="firstPage">First</button>
            <button id="prevPage">Previous</button>
            <span id="pageInfo"></span>
            <button id="nextPage">Next</button>
            <button id="lastPage">Last</button>
        </div>
    </div>

    <script>
        class WeatherDataPagination {
            constructor() {
                this.currentPage = 1;
                this.pageSize = 10;
                this.sortBy = '';
                this.sortDirection = 'Ascending';
                this.pagination = null;
                
                this.initializeEventListeners();
                this.loadData();
            }
            
            initializeEventListeners() {
                document.getElementById('pageSize').addEventListener('change', (e) => {
                    this.pageSize = parseInt(e.target.value);
                    this.currentPage = 1;
                    this.loadData();
                });
                
                document.getElementById('sortBy').addEventListener('change', (e) => {
                    this.sortBy = e.target.value;
                    this.currentPage = 1;
                    this.loadData();
                });
                
                document.getElementById('sortDirection').addEventListener('click', () => {
                    this.sortDirection = this.sortDirection === 'Ascending' ? 'Descending' : 'Ascending';
                    document.getElementById('sortDirection').textContent = `${this.sortDirection} ↕️`;
                    this.currentPage = 1;
                    this.loadData();
                });
                
                document.getElementById('firstPage').addEventListener('click', () => this.goToPage(1));
                document.getElementById('prevPage').addEventListener('click', () => this.goToPage(this.currentPage - 1));
                document.getElementById('nextPage').addEventListener('click', () => this.goToPage(this.currentPage + 1));
                document.getElementById('lastPage').addEventListener('click', () => this.goToPage(this.pagination?.totalPages || 1));
            }
            
            async loadData() {
                this.showLoading(true);
                this.hideError();
                
                try {
                    const request = {
                        city: "London",
                        page: this.currentPage,
                        pageSize: this.pageSize,
                        sortBy: this.sortBy || undefined,
                        sortDirection: this.sortDirection
                    };
                    
                    const API_BASE_URL = window.location.hostname === 'localhost' 
                        ? 'http://localhost:5249/level4'
                        : 'https://api.alexandergarin.com/level4';
                        
                    const response = await fetch(`${API_BASE_URL}/weather-data/paginated`, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(request)
                    });
                    
                    if (!response.ok) {
                        throw new Error(`HTTP error! status: ${response.status}`);
                    }
                    
                    const result = await response.json();
                    this.renderData(result.data);
                    this.renderPagination(result.pagination);
                    this.pagination = result.pagination;
                    
                } catch (error) {
                    this.showError(error.message);
                } finally {
                    this.showLoading(false);
                }
            }
            
            renderData(data) {
                const tbody = document.getElementById('tableBody');
                tbody.innerHTML = '';
                
                data.forEach(item => {
                    const row = document.createElement('tr');
                    row.innerHTML = `
                        <td>${item.id.substring(0, 8)}...</td>
                        <td>${item.city}</td>
                        <td>${item.country}</td>
                        <td>${item.temperatureC}°C</td>
                        <td>${item.summary}</td>
                        <td>${new Date(item.date).toLocaleDateString()}</td>
                    `;
                    tbody.appendChild(row);
                });
                
                document.getElementById('dataTable').style.display = data.length > 0 ? 'table' : 'none';
            }
            
            renderPagination(pagination) {
                document.getElementById('pageInfo').textContent = 
                    `Page ${pagination.currentPage} of ${pagination.totalPages} (${pagination.totalItems} items)`;
                
                document.getElementById('firstPage').disabled = !pagination.hasPreviousPage;
                document.getElementById('prevPage').disabled = !pagination.hasPreviousPage;
                document.getElementById('nextPage').disabled = !pagination.hasNextPage;
                document.getElementById('lastPage').disabled = !pagination.hasNextPage;
                
                document.getElementById('pagination').style.display = 'block';
            }
            
            goToPage(page) {
                if (page >= 1 && page <= (this.pagination?.totalPages || 1)) {
                    this.currentPage = page;
                    this.loadData();
                }
            }
            
            showLoading(show) {
                document.getElementById('loading').style.display = show ? 'block' : 'none';
                document.getElementById('dataTable').style.display = show ? 'none' : 'table';
                document.getElementById('pagination').style.display = show ? 'none' : 'block';
            }
            
            showError(message) {
                const errorDiv = document.getElementById('error');
                errorDiv.textContent = `Error: ${message}`;
                errorDiv.style.display = 'block';
            }
            
            hideError() {
                document.getElementById('error').style.display = 'none';
            }
        }
        
        // Initialize when page loads
        document.addEventListener('DOMContentLoaded', () => {
            new WeatherDataPagination();
        });
    </script>
</body>
</html>
```

---

## 🧪 Quick Testing with curl

```bash
# Test weather data pagination (Production)
curl -X POST https://api.alexandergarin.com/level4/weather-data/paginated \
  -H "Content-Type: application/json" \
  -d '{"city": "London", "page": 1, "pageSize": 5, "sortBy": "date"}' | jq '.'

# Test cities search (Production)
curl -X POST https://api.alexandergarin.com/level4/cities/browse \
  -H "Content-Type: application/json" \
  -d '{"page": 1, "pageSize": 10, "searchQuery": "London"}' | jq '.'

# Test with GET parameters (Production)
curl "https://api.alexandergarin.com/level4/cities/browse?page=1&pageSize=5&searchQuery=New" | jq '.'

# Local development examples
curl -X POST http://localhost:5249/level4/weather-data/paginated \
  -H "Content-Type: application/json" \
  -d '{"city": "London", "page": 1, "pageSize": 5, "sortBy": "date"}' | jq '.'
```

---

## 🎯 Integration Checklist

- [ ] **API Base URL configured** (Production: `https://api.alexandergarin.com/level4`, Local: `http://localhost:5249/level4`)
- [ ] **TypeScript interfaces defined** for requests and responses
- [ ] **HTTP client setup** with proper headers (`Content-Type: application/json`)
- [ ] **Error handling implemented** for network and API errors
- [ ] **Loading states managed** for better user experience
- [ ] **Pagination controls created** (next, previous, page size)
- [ ] **Sorting options added** if needed for your use case
- [ ] **Responsive design considered** for mobile devices
- [ ] **URL state management** (optional, for bookmarkable pages)
- [ ] **Testing implemented** for pagination logic

---

**🚀 Start with the weather data endpoint (`/level4/weather-data/paginated`) to test your integration, then adapt the same patterns to other endpoints. The universal response format ensures consistency across all paginated data!** 