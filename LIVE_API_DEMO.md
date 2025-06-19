# 🌐 Live API Demo - Connect to Production

Your Level 4 pagination system is now live at **[https://api.alexandergarin.com/](https://api.alexandergarin.com/)**!

## 🚀 Quick Test - No Installation Required

### Test the API Directly in Your Browser

**1. Check API Health:**
```
https://api.alexandergarin.com/health
```

**2. Get Pagination Info:**
```
https://api.alexandergarin.com/level4/pagination/info
```

**3. Browse Cities (GET):**
```
https://api.alexandergarin.com/level4/cities/browse?page=1&pageSize=5&searchQuery=London
```

### Test with curl (Copy & Paste Ready)

```bash
# Health Check
curl -s "https://api.alexandergarin.com/health"

# Demo Pagination
curl -s -X POST "https://api.alexandergarin.com/level4/paginate/demo" \
  -H "Content-Type: application/json" \
  -d '{"page": 1, "pageSize": 5, "sortBy": "value"}' | jq '.'

# Cities Search
curl -s -X POST "https://api.alexandergarin.com/level4/cities/browse" \
  -H "Content-Type: application/json" \
  -d '{"page": 1, "pageSize": 10, "searchQuery": "New York"}' | jq '.'

# Weather History Pagination
curl -s -X POST "https://api.alexandergarin.com/level4/weather/history/paginated" \
  -H "Content-Type: application/json" \
  -d '{"city": "London", "page": 1, "pageSize": 5}' | jq '.'

# Supabase Weather Demo
curl -s -X POST "https://api.alexandergarin.com/level4/supabase/weather" \
  -H "Content-Type: application/json" \
  -d '{"page": 1, "pageSize": 3, "city": "London"}' | jq '.'

# Supabase Table Info
curl -s "https://api.alexandergarin.com/level4/supabase/table-info" | jq '.'
```

## 📱 Frontend Integration Examples

### React Hook for Production API

```tsx
import { useState, useEffect } from 'react';

const API_BASE_URL = 'https://api.alexandergarin.com/level4';

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

interface DemoItem {
  id: number;
  name: string;
  category: string;
  value: number;
  date: string;
}

export function useProductionAPI() {
  const [data, setData] = useState<DemoItem[]>([]);
  const [pagination, setPagination] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchDemoData = async (page = 1, pageSize = 10) => {
    setLoading(true);
    setError(null);

    try {
      const response = await fetch(`${API_BASE_URL}/paginate/demo`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ page, pageSize, sortBy: 'value' })
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const result: PaginatedResponse<DemoItem> = await response.json();
      setData(result.data);
      setPagination(result.pagination);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDemoData();
  }, []);

  return { data, pagination, loading, error, fetchDemoData };
}

// Usage Component
export const LiveAPIDemo: React.FC = () => {
  const { data, pagination, loading, error, fetchDemoData } = useProductionAPI();

  if (loading) return <div>Loading from production API...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div className="live-api-demo">
      <h2>Live Production API Demo 🚀</h2>
      <p>Connected to: <strong>https://api.alexandergarin.com/</strong></p>
      
      <div className="controls">
        <button onClick={() => fetchDemoData(1, 5)}>Load 5 items</button>
        <button onClick={() => fetchDemoData(1, 10)}>Load 10 items</button>
        <button onClick={() => fetchDemoData(2, 5)}>Page 2</button>
      </div>

      <table>
        <thead>
          <tr>
            <th>ID</th>
            <th>Name</th>
            <th>Category</th>
            <th>Value</th>
            <th>Date</th>
          </tr>
        </thead>
        <tbody>
          {data.map(item => (
            <tr key={item.id}>
              <td>{item.id}</td>
              <td>{item.name}</td>
              <td>{item.category}</td>
              <td>{item.value}</td>
              <td>{new Date(item.date).toLocaleDateString()}</td>
            </tr>
          ))}
        </tbody>
      </table>

      {pagination && (
        <div className="pagination-info">
          <p>
            Page {pagination.currentPage} of {pagination.totalPages} 
            ({pagination.totalItems} total items)
          </p>
          <p>
            Showing items {pagination.firstItemIndex} to {pagination.lastItemIndex}
          </p>
        </div>
      )}
    </div>
  );
};
```

### Vue.js Example

```vue
<template>
  <div class="live-api-demo">
    <h2>Live Production API Demo 🚀</h2>
    <p>Connected to: <strong>https://api.alexandergarin.com/</strong></p>
    
    <div v-if="loading">Loading from production API...</div>
    <div v-else-if="error" class="error">Error: {{ error }}</div>
    
    <div v-else>
      <div class="controls">
        <button @click="fetchData(1, 5)">Load 5 items</button>
        <button @click="fetchData(1, 10)">Load 10 items</button>
        <button @click="fetchData(2, 5)">Page 2</button>
      </div>

      <table>
        <thead>
          <tr>
            <th>ID</th>
            <th>Name</th>
            <th>Category</th>
            <th>Value</th>
            <th>Date</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in data" :key="item.id">
            <td>{{ item.id }}</td>
            <td>{{ item.name }}</td>
            <td>{{ item.category }}</td>
            <td>{{ item.value }}</td>
            <td>{{ new Date(item.date).toLocaleDateString() }}</td>
          </tr>
        </tbody>
      </table>

      <div v-if="pagination" class="pagination-info">
        <p>
          Page {{ pagination.currentPage }} of {{ pagination.totalPages }} 
          ({{ pagination.totalItems }} total items)
        </p>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from 'vue';

export default {
  name: 'LiveAPIDemo',
  setup() {
    const data = ref([]);
    const pagination = ref(null);
    const loading = ref(false);
    const error = ref(null);

    const fetchData = async (page = 1, pageSize = 10) => {
      loading.value = true;
      error.value = null;

      try {
        const response = await fetch('https://api.alexandergarin.com/level4/paginate/demo', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ page, pageSize, sortBy: 'value' })
        });

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();
        data.value = result.data;
        pagination.value = result.pagination;
      } catch (err) {
        error.value = err.message || 'Failed to fetch data';
      } finally {
        loading.value = false;
      }
    };

    onMounted(() => {
      fetchData();
    });

    return { data, pagination, loading, error, fetchData };
  }
};
</script>
```

## 🌍 Available Endpoints on Production

### Core Pagination Endpoints

1. **Demo Data**: `POST /level4/paginate/demo`
2. **Cities Browse**: `GET/POST /level4/cities/browse`
3. **Autocomplete**: `POST /level4/autocomplete/paginated`
4. **Weather History**: `POST /level4/weather/history/paginated`
5. **Bulk Operations**: `POST /level4/weather/bulk/paginated`
6. **Pagination Info**: `GET /level4/pagination/info`

### Supabase Integration

7. **Supabase Weather**: `POST /level4/supabase/weather`
8. **Table Info**: `GET /level4/supabase/table-info`

### Health & Status

9. **Health Check**: `GET /health`
10. **Root**: `GET /` (shows available levels)

## 🎯 What This Means

✅ **Your pagination system is live and accessible globally**  
✅ **Any frontend can connect to it immediately**  
✅ **No CORS issues - properly configured for web apps**  
✅ **Consistent API responses across all endpoints**  
✅ **Production-ready with error handling**  

## 🚀 Next Steps

1. **Test the API** using the curl examples above
2. **Integrate with your frontend** using the React/Vue examples
3. **Connect your real Supabase data** following the integration guide
4. **Build your UI** with consistent pagination across all data sources

Your universal pagination system is now powering a live API that can handle any data source! 🎉 