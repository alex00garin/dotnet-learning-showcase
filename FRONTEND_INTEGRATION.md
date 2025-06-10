# Frontend Integration Guide - Level 3 Autocomplete System

## 🚀 Quick Start

```javascript
// Basic city autocomplete
const response = await fetch('/level3/cities/autocomplete?query=ber&limit=5');
const cities = await response.json();
console.log(cities); // ['Berlin', 'Bergen', 'Berkeley', 'Bern', 'Bermuda']
```

## 📋 API Endpoints Reference

### 1. City Autocomplete (Recommended)
**Endpoint:** `GET /level3/cities/autocomplete`

**Parameters:**
- `query` (required): Partial city name (minimum 1 character)
- `limit` (optional): Number of results (default: 10, max: 50)

**Response:** Rich autocomplete object
```json
{
  "query": "ber",
  "results": [
    {
      "id": "Berd_AM",
      "label": "Berd, 09, AM",
      "value": "Berd", 
      "metadata": {
        "country": "AM",
        "latitude": "40.88066",
        "longitude": "45.38901"
      }
    }
  ],
  "totalCount": 5,
  "dataSource": "cities"
}

**Examples:**
```javascript
// Basic search
fetch('/level3/cities/autocomplete?query=par')

// Limited results
fetch('/level3/cities/autocomplete?query=san&limit=3')
```

### 2. Generic Autocomplete
**Endpoint:** `POST /level3/autocomplete`

**Request Body:**
```json
{
  "dataSource": "cities",
  "query": "ber",
  "limit": 5
}
```

**Response:** Same as city autocomplete
```json
["Berlin", "Bergen", "Berkeley", "Bern", "Bermuda"]
```

### 3. Data Sources Management
**Endpoint:** `GET /level3/datasources`

**Response:**
```json
[
  {
    "name": "cities",
    "description": "World cities database",
    "itemCount": 154694,
    "isHealthy": true
  },
  {
    "name": "countries", 
    "description": "Country names",
    "itemCount": 195,
    "isHealthy": true
  }
]
```

### 4. Health Check
**Endpoint:** `GET /level3/datasources/{dataSource}/health`

**Response:**
```json
{
  "dataSource": "cities",
  "isHealthy": true,
  "itemCount": 154694,
  "lastUpdated": "2024-01-15T10:30:00Z"
}
```

## 🎯 React Integration

```jsx
import React, { useState, useEffect, useCallback } from 'react';

const CityAutocomplete = ({ onSelect, placeholder = "Enter city name..." }) => {
  const [query, setQuery] = useState('');
  const [suggestions, setSuggestions] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  // Debounced search function
  const searchCities = useCallback(
    debounce(async (searchQuery) => {
      if (searchQuery.length < 1) {
        setSuggestions([]);
        return;
      }

      setLoading(true);
      setError(null);

      try {
        const response = await fetch(
          `/level3/cities/autocomplete?query=${encodeURIComponent(searchQuery)}&limit=8`
        );
        
        if (!response.ok) {
          throw new Error(`HTTP ${response.status}`);
        }

        const data = await response.json();
        // Extract city names/labels from the response
        const cities = data.results?.map(city => city.label || city.value) || [];
        setSuggestions(cities);
      } catch (err) {
        setError('Failed to load suggestions');
        setSuggestions([]);
      } finally {
        setLoading(false);
      }
    }, 300),
    []
  );

  useEffect(() => {
    searchCities(query);
  }, [query, searchCities]);

  const handleSelect = (city) => {
    setQuery(city);
    setSuggestions([]);
    onSelect?.(city);
  };

  return (
    <div className="autocomplete-container">
      <input
        type="text"
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        placeholder={placeholder}
        className="autocomplete-input"
      />
      
      {loading && <div className="autocomplete-loading">Searching...</div>}
      {error && <div className="autocomplete-error">{error}</div>}
      
      {suggestions.length > 0 && (
        <ul className="autocomplete-suggestions">
          {suggestions.map((city, index) => (
            <li
              key={index}
              onClick={() => handleSelect(city)}
              className="autocomplete-suggestion"
            >
              {city}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
};

// Debounce utility
function debounce(func, wait) {
  let timeout;
  return function executedFunction(...args) {
    const later = () => {
      clearTimeout(timeout);
      func(...args);
    };
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
  };
}

export default CityAutocomplete;
```

## 🎨 CSS Styling

```css
.autocomplete-container {
  position: relative;
  width: 300px;
}

.autocomplete-input {
  width: 100%;
  padding: 12px 16px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  font-size: 16px;
  outline: none;
  transition: border-color 0.2s ease;
}

.autocomplete-input:focus {
  border-color: #007bff;
  box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.1);
}

.autocomplete-suggestions {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: white;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  list-style: none;
  margin: 4px 0 0 0;
  padding: 0;
  max-height: 200px;
  overflow-y: auto;
  z-index: 1000;
}

.autocomplete-suggestion {
  padding: 12px 16px;
  cursor: pointer;
  border-bottom: 1px solid #f0f0f0;
  transition: background-color 0.2s ease;
}

.autocomplete-suggestion:hover {
  background-color: #f8f9fa;
}

.autocomplete-suggestion:last-child {
  border-bottom: none;
}

.autocomplete-loading {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: white;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 12px 16px;
  margin: 4px 0 0 0;
  font-size: 14px;
  color: #666;
  z-index: 1000;
}

.autocomplete-error {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: #fff3f3;
  border: 1px solid #ffcdd2;
  border-radius: 8px;
  padding: 12px 16px;
  margin: 4px 0 0 0;
  font-size: 14px;
  color: #d32f2f;
  z-index: 1000;
}
```

## 📱 Vanilla JavaScript Implementation

```javascript
class CityAutocomplete {
  constructor(inputElement, options = {}) {
    this.input = inputElement;
    this.options = {
      limit: 8,
      minChars: 1,
      debounceMs: 300,
      onSelect: () => {},
      ...options
    };
    
    this.suggestions = [];
    this.timeoutId = null;
    
    this.init();
  }

  init() {
    this.createSuggestionsList();
    this.input.addEventListener('input', this.handleInput.bind(this));
    this.input.addEventListener('blur', this.hideSuggestions.bind(this));
  }

  createSuggestionsList() {
    this.suggestionsList = document.createElement('ul');
    this.suggestionsList.className = 'autocomplete-suggestions';
    this.suggestionsList.style.display = 'none';
    this.input.parentNode.appendChild(this.suggestionsList);
  }

  handleInput(event) {
    const query = event.target.value;
    
    clearTimeout(this.timeoutId);
    this.timeoutId = setTimeout(() => {
      this.search(query);
    }, this.options.debounceMs);
  }

  async search(query) {
    if (query.length < this.options.minChars) {
      this.hideSuggestions();
      return;
    }

    try {
      const response = await fetch(
        `/level3/cities/autocomplete?query=${encodeURIComponent(query)}&limit=${this.options.limit}`
      );
      
      if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
      }

      const data = await response.json();
      // Extract city labels for display
      this.suggestions = data.results?.map(city => city.label || city.value) || [];
      this.showSuggestions();
    } catch (error) {
      console.error('Autocomplete search failed:', error);
      this.hideSuggestions();
    }
  }

  showSuggestions() {
    this.suggestionsList.innerHTML = '';
    
    if (this.suggestions.length === 0) {
      this.hideSuggestions();
      return;
    }

    this.suggestions.forEach(city => {
      const li = document.createElement('li');
      li.textContent = city;
      li.className = 'autocomplete-suggestion';
      li.addEventListener('click', () => this.selectCity(city));
      this.suggestionsList.appendChild(li);
    });

    this.suggestionsList.style.display = 'block';
  }

  hideSuggestions() {
    setTimeout(() => {
      this.suggestionsList.style.display = 'none';
    }, 150);
  }

  selectCity(city) {
    this.input.value = city;
    this.hideSuggestions();
    this.options.onSelect(city);
  }
}

// Usage Example
const input = document.getElementById('city-input');
const autocomplete = new CityAutocomplete(input, {
  limit: 5,
  onSelect: (city) => {
    console.log('Selected city:', city);
    // Integrate with weather API
    fetchWeather(city);
  }
});
```

## 🚀 Performance Optimizations

### Request Cancellation
```javascript
class OptimizedAutocomplete {
  constructor() {
    this.currentController = null;
  }

  async search(query) {
    // Cancel previous request
    if (this.currentController) {
      this.currentController.abort();
    }

    this.currentController = new AbortController();

    try {
      const response = await fetch(`/level3/cities/autocomplete?query=${query}`, {
        signal: this.currentController.signal
      });
      
      return await response.json();
    } catch (error) {
      if (error.name === 'AbortError') {
        return []; // Request cancelled
      }
      throw error;
    }
  }
}
```

### Result Caching
```javascript
class CachedAutocomplete {
  constructor() {
    this.cache = new Map();
    this.cacheTimeout = 5 * 60 * 1000; // 5 minutes
  }

  async search(query) {
    const cacheKey = query.toLowerCase();
    const cached = this.cache.get(cacheKey);
    
    if (cached && Date.now() - cached.timestamp < this.cacheTimeout) {
      return cached.results;
    }

    const results = await this.fetchResults(query);
    this.cache.set(cacheKey, {
      results,
      timestamp: Date.now()
    });

    return results;
  }
}
```

## 🔧 Error Handling

### HTTP Status Codes
- `200` - Success
- `400` - Bad Request (invalid parameters)
- `500` - Internal server error

### Robust Error Handling
```javascript
async function searchWithErrorHandling(query) {
  try {
    const response = await fetch(`/level3/cities/autocomplete?query=${query}`);
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    if (error.name === 'AbortError') {
      return []; // Request cancelled
    }
    
    console.error('Autocomplete error:', error);
    return []; // Return empty array as fallback
  }
}
```

## 🌐 Integration with Other Levels

### Weather Integration
```javascript
// After city selection, fetch weather
async function onCitySelect(city) {
  try {
    // Get current weather
    const weatherResponse = await fetch(`/level1/weatherforecast?city=${encodeURIComponent(city)}`);
    const weather = await weatherResponse.json();
    displayWeather(weather);

    // Check for historical data
    const historyResponse = await fetch(`/level2/weatherforecast/history/${encodeURIComponent(city)}`);
    if (historyResponse.ok) {
      const history = await historyResponse.json();
      displayHistory(history);
    }
  } catch (error) {
    console.error('Weather integration failed:', error);
  }
}
```

## 📊 Data Source Information

### Cities Dataset
- **Total Cities**: 154,694 worldwide
- **Coverage**: Global (all countries)
- **Language**: English city names
- **Performance**: ~2 second initial load, then instant
- **Offline Support**: ✅ Available after first load

### Available Data Sources
- `"cities"` - World cities (154,694 entries)
- `"countries"` - Country names (195 entries)

## 🚀 Getting Started

1. **Choose your implementation**: React, Vanilla JS, or other framework
2. **Copy the relevant code** from the examples above
3. **Add the CSS styling** to match your design
4. **Test the integration** with different search queries
5. **Add error handling** for production use
6. **Optimize performance** with debouncing and caching

## 💡 Best Practices

- **Debounce input**: Use 300ms delay to reduce API calls
- **Limit results**: Show 5-10 suggestions maximum
- **Handle errors gracefully**: Always provide fallbacks
- **Cancel previous requests**: Prevent race conditions
- **Cache results**: Optional for better performance
- **Mobile-friendly**: Consider touch interactions

This guide provides everything needed to integrate the Level 3 autocomplete system into any frontend application! 