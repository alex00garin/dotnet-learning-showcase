# Frontend Integration Guide - Hourly Weather API

This guide provides everything frontend developers need to integrate with the new hourly weather forecast endpoints.

## 🚀 Quick Start

### Base URL
```
http://localhost:5249
```

### Authentication
No authentication required for weather endpoints.

## 📋 Available Endpoints

### 1. Basic Hourly Weather Forecast
```http
GET /level1/weatherforecast/hourly?city={cityName}
```

### 2. Smart Hourly Weather Forecast (with autocomplete)
```http
GET /level1/weatherforecast/hourly/smart?city={cityName}
```

### 3. City Autocomplete
```http
GET /level3/cities/autocomplete?query={partialCityName}
```

## 📊 Data Structures

### HourlyWeatherResponse
```typescript
interface HourlyWeatherResponse {
  location: string;              // "Berlin, Germany"
  hourlyForecasts: HourlyWeatherForecast[];
}
```

### HourlyWeatherForecast
```typescript
interface HourlyWeatherForecast {
  time: string;                  // ISO 8601: "2024-01-15T14:00:00"
  temperatureC: number;          // Temperature in Celsius: 22.5
  temperatureF: number;          // Temperature in Fahrenheit: 72.5 (auto-calculated)
  precipitation: number;         // Precipitation in mm: 0.2
  weatherCode: number;           // WMO weather code: 61
  weatherDescription: string;    // Human-readable: "Rain"
  windSpeed?: number | null;     // Wind speed in km/h: 15.2 (optional)
  humidity?: number | null;      // Relative humidity %: 65.0 (optional)
  apparentTemperature?: number | null; // Feels-like temp °C: 20.8 (optional)
}
```

### Smart Response (Success)
```typescript
interface SmartWeatherResponse {
  weather: HourlyWeatherResponse;
  cityFound: boolean;            // true
  message?: string;
  suggestions?: AutocompleteItem[];
  hint?: string;
}
```

### Smart Response (Error with Suggestions)
```typescript
interface SmartWeatherErrorResponse {
  weather: null;
  cityFound: boolean;            // false
  message: string;               // "City 'NotACity' not found."
  suggestions: AutocompleteItem[];
  hint: string;                  // "Try using autocomplete..."
}
```

### AutocompleteItem
```typescript
interface AutocompleteItem {
  name: string;                  // "Berlin"
  fullName: string;              // "Berlin, Germany"
  country: string;               // "Germany"
  latitude: number;              // 52.52437
  longitude: number;             // 13.41053
  relevanceScore: number;        // 0.95 (0-1)
}
```

### Weather Code Mappings
```typescript
const WEATHER_DESCRIPTIONS: Record<number, string> = {
  0: "Clear sky",
  1: "Mainly clear",
  2: "Partly cloudy",
  3: "Overcast",
  45: "Foggy",
  48: "Foggy",
  51: "Drizzle",
  53: "Drizzle", 
  55: "Drizzle",
  61: "Rain",
  63: "Rain",
  65: "Rain",
  71: "Snow",
  73: "Snow",
  75: "Snow",
  80: "Rain showers",
  81: "Rain showers",
  82: "Rain showers",
  95: "Thunderstorm"
  // Default: "Unknown"
};
```

## 🔗 API Examples

### Basic Hourly Forecast
```javascript
// Fetch hourly weather for Berlin
const response = await fetch('/level1/weatherforecast/hourly?city=Berlin');
const data = await response.json();

// Example response:
{
  "location": "Berlin, Germany",
  "hourlyForecasts": [
    {
      "time": "2024-01-15T14:00:00",
      "temperatureC": 22.5,
      "temperatureF": 72.5,
      "precipitation": 0.0,
      "weatherCode": 0,
      "weatherDescription": "Clear sky",
      "windSpeed": 15.2,
      "humidity": 65.0,
      "apparentTemperature": 20.8
    }
    // ... 23 more hours
  ]
}
```

### Smart Forecast (Success)
```javascript
const response = await fetch('/level1/weatherforecast/hourly/smart?city=Berl');
const data = await response.json();

// Response includes weather data + metadata
{
  "weather": {
    "location": "Berlin, Germany",
    "hourlyForecasts": [/* 24 hours of data */]
  },
  "cityFound": true
}
```

### Smart Forecast (Error with Suggestions)
```javascript
const response = await fetch('/level1/weatherforecast/hourly/smart?city=NotACity');
// Status: 404
const data = await response.json();

{
  "weather": null,
  "cityFound": false,
  "message": "City 'NotACity' not found.",
  "suggestions": [
    {
      "name": "New York",
      "fullName": "New York, United States",
      "country": "United States",
      "latitude": 40.71427,
      "longitude": -74.00597,
      "relevanceScore": 0.85
    }
    // ... more suggestions
  ],
  "hint": "Try using autocomplete: /level3/cities/autocomplete?query=NotACity"
}
```

### City Autocomplete
```javascript
const response = await fetch('/level3/cities/autocomplete?query=Lond');
const suggestions = await response.json();

// Array of city suggestions
[
  {
    "name": "London",
    "fullName": "London, United Kingdom", 
    "country": "United Kingdom",
    "latitude": 51.50853,
    "longitude": -0.12574,
    "relevanceScore": 0.98
  },
  {
    "name": "London",
    "fullName": "London, Canada",
    "country": "Canada", 
    "latitude": 42.98339,
    "longitude": -81.23304,
    "relevanceScore": 0.92
  }
  // ... more matches
]
```

## 🎨 React Component Examples

### Basic Weather Display
```tsx
import React, { useState, useEffect } from 'react';

interface WeatherDisplayProps {
  city: string;
}

const HourlyWeatherDisplay: React.FC<WeatherDisplayProps> = ({ city }) => {
  const [weather, setWeather] = useState<HourlyWeatherResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchWeather = async () => {
      setLoading(true);
      setError(null);
      
      try {
        const response = await fetch(`/level1/weatherforecast/hourly?city=${encodeURIComponent(city)}`);
        
        if (!response.ok) {
          throw new Error(`City '${city}' not found`);
        }
        
        const data: HourlyWeatherResponse = await response.json();
        setWeather(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to fetch weather');
      } finally {
        setLoading(false);
      }
    };

    if (city) {
      fetchWeather();
    }
  }, [city]);

  if (loading) return <div>Loading weather data...</div>;
  if (error) return <div>Error: {error}</div>;
  if (!weather) return null;

  return (
    <div className="weather-display">
      <h2>📍 {weather.location}</h2>
      <div className="hourly-grid">
        {weather.hourlyForecasts.map((forecast, index) => (
          <div key={index} className="hour-card">
            <div className="time">{new Date(forecast.time).toLocaleTimeString('en-US', { hour: '2-digit' })}</div>
            <div className="temp">{Math.round(forecast.temperatureC)}°C</div>
            <div className="description">{forecast.weatherDescription}</div>
            {forecast.precipitation > 0 && (
              <div className="precipitation">💧 {forecast.precipitation}mm</div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
};
```

### Smart Weather with Autocomplete
```tsx
import React, { useState, useEffect, useCallback } from 'react';
import { debounce } from 'lodash';

const SmartWeatherSearch: React.FC = () => {
  const [query, setQuery] = useState('');
  const [suggestions, setSuggestions] = useState<AutocompleteItem[]>([]);
  const [weather, setWeather] = useState<HourlyWeatherResponse | null>(null);
  const [showSuggestions, setShowSuggestions] = useState(false);

  // Debounced autocomplete
  const fetchSuggestions = useCallback(
    debounce(async (searchQuery: string) => {
      if (searchQuery.length >= 2) {
        try {
          const response = await fetch(`/level3/cities/autocomplete?query=${encodeURIComponent(searchQuery)}`);
          const data: AutocompleteItem[] = await response.json();
          setSuggestions(data.slice(0, 5)); // Show top 5
          setShowSuggestions(true);
        } catch (err) {
          console.error('Autocomplete failed:', err);
        }
      } else {
        setSuggestions([]);
        setShowSuggestions(false);
      }
    }, 300),
    []
  );

  useEffect(() => {
    fetchSuggestions(query);
  }, [query, fetchSuggestions]);

  const handleCitySelect = async (city: AutocompleteItem) => {
    setQuery(city.fullName);
    setShowSuggestions(false);
    
    try {
      const response = await fetch(`/level1/weatherforecast/hourly/smart?city=${encodeURIComponent(city.name)}`);
      const data: SmartWeatherResponse = await response.json();
      
      if (data.cityFound && data.weather) {
        setWeather(data.weather);
      }
    } catch (err) {
      console.error('Weather fetch failed:', err);
    }
  };

  return (
    <div className="smart-weather-search">
      <div className="search-container">
        <input
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Search for a city..."
          className="city-search"
        />
        
        {showSuggestions && suggestions.length > 0 && (
          <div className="suggestions-dropdown">
            {suggestions.map((suggestion, index) => (
              <div
                key={index}
                className="suggestion-item"
                onClick={() => handleCitySelect(suggestion)}
              >
                <span className="city-name">{suggestion.name}</span>
                <span className="country">{suggestion.country}</span>
              </div>
            ))}
          </div>
        )}
      </div>

      {weather && <HourlyWeatherDisplay city={weather.location} />}
    </div>
  );
};
```

### Weather Chart Component
```tsx
import React from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

interface WeatherChartProps {
  forecasts: HourlyWeatherForecast[];
}

const WeatherChart: React.FC<WeatherChartProps> = ({ forecasts }) => {
  const chartData = forecasts.map(forecast => ({
    time: new Date(forecast.time).toLocaleTimeString('en-US', { hour: '2-digit' }),
    temperature: forecast.temperatureC,
    precipitation: forecast.precipitation,
    humidity: forecast.humidity || 0
  }));

  return (
    <div className="weather-chart">
      <h3>📈 24-Hour Temperature Trend</h3>
      <ResponsiveContainer width="100%" height={300}>
        <LineChart data={chartData}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="time" />
          <YAxis />
          <Tooltip 
            formatter={(value, name) => [
              `${value}${name === 'temperature' ? '°C' : name === 'precipitation' ? 'mm' : '%'}`,
              name.charAt(0).toUpperCase() + name.slice(1)
            ]}
          />
          <Line 
            type="monotone" 
            dataKey="temperature" 
            stroke="#8884d8" 
            strokeWidth={2}
            name="temperature"
          />
          <Line 
            type="monotone" 
            dataKey="precipitation" 
            stroke="#82ca9d" 
            strokeWidth={2}
            name="precipitation"
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
};
```

## 🎯 Implementation Tips

### Error Handling
```typescript
const fetchWeatherWithErrorHandling = async (city: string) => {
  try {
    const response = await fetch(`/level1/weatherforecast/hourly/smart?city=${encodeURIComponent(city)}`);
    
    if (response.ok) {
      const data: SmartWeatherResponse = await response.json();
      if (data.cityFound) {
        return { success: true, weather: data.weather };
      }
    } else if (response.status === 404) {
      const errorData: SmartWeatherErrorResponse = await response.json();
      return { 
        success: false, 
        error: errorData.message,
        suggestions: errorData.suggestions 
      };
    }
    
    throw new Error(`HTTP ${response.status}`);
  } catch (error) {
    return { 
      success: false, 
      error: 'Network error or service unavailable' 
    };
  }
};
```

### Temperature Utils
```typescript
const temperatureUtils = {
  celsiusToFahrenheit: (celsius: number): number => {
    return Math.round((celsius * 9/5) + 32);
  },
  
  formatTemperature: (celsius: number, unit: 'C' | 'F' = 'C'): string => {
    const temp = unit === 'F' ? temperatureUtils.celsiusToFahrenheit(celsius) : Math.round(celsius);
    return `${temp}°${unit}`;
  },
  
  getTemperatureColor: (celsius: number): string => {
    if (celsius < 0) return '#0066cc';      // Cold - Blue
    if (celsius < 10) return '#00aaff';     // Cool - Light Blue  
    if (celsius < 20) return '#00cc00';     // Mild - Green
    if (celsius < 30) return '#ffaa00';     // Warm - Orange
    return '#ff4400';                       // Hot - Red
  }
};
```

### Weather Icon Mapping
```typescript
const getWeatherIcon = (weatherCode: number): string => {
  const iconMap: Record<number, string> = {
    0: '☀️',   // Clear sky
    1: '🌤️',   // Mainly clear
    2: '⛅',   // Partly cloudy
    3: '☁️',   // Overcast
    45: '🌫️',  // Foggy
    48: '🌫️',  // Foggy
    51: '🌦️',  // Drizzle
    53: '🌦️',  // Drizzle
    55: '🌦️',  // Drizzle
    61: '🌧️',  // Rain
    63: '🌧️',  // Rain
    65: '🌧️',  // Rain
    71: '🌨️',  // Snow
    73: '🌨️',  // Snow
    75: '🌨️',  // Snow
    80: '🌦️',  // Rain showers
    81: '🌦️',  // Rain showers
    82: '🌦️',  // Rain showers
    95: '⛈️'   // Thunderstorm
  };
  
  return iconMap[weatherCode] || '❓';
};
```

## 📱 Mobile Considerations

### Responsive Design
```css
.weather-display {
  display: grid;
  gap: 1rem;
}

.hourly-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(80px, 1fr));
  gap: 0.5rem;
  overflow-x: auto;
  padding-bottom: 1rem;
}

@media (max-width: 768px) {
  .hourly-grid {
    grid-template-columns: repeat(24, minmax(60px, 1fr));
    scroll-snap-type: x mandatory;
  }
  
  .hour-card {
    scroll-snap-align: start;
  }
}
```

### Touch-Friendly Autocomplete
```css
.suggestion-item {
  padding: 1rem;
  min-height: 44px; /* iOS touch target */
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #eee;
  cursor: pointer;
}

.suggestion-item:hover,
.suggestion-item:focus {
  background-color: #f5f5f5;
}
```

## 🔄 Real-time Updates

### Polling Strategy
```typescript
const useWeatherPolling = (city: string, intervalMs: number = 300000) => { // 5 minutes
  const [weather, setWeather] = useState<HourlyWeatherResponse | null>(null);
  
  useEffect(() => {
    const fetchWeather = async () => {
      try {
        const response = await fetch(`/level1/weatherforecast/hourly?city=${encodeURIComponent(city)}`);
        if (response.ok) {
          const data = await response.json();
          setWeather(data);
        }
      } catch (error) {
        console.error('Weather polling failed:', error);
      }
    };

    fetchWeather(); // Initial fetch
    const interval = setInterval(fetchWeather, intervalMs);
    
    return () => clearInterval(interval);
  }, [city, intervalMs]);

  return weather;
};
```

## 🎛️ State Management (Redux Example)

### Weather Slice
```typescript
import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';

export const fetchHourlyWeather = createAsyncThunk(
  'weather/fetchHourly',
  async (city: string) => {
    const response = await fetch(`/level1/weatherforecast/hourly/smart?city=${encodeURIComponent(city)}`);
    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Failed to fetch weather');
    }
    return response.json();
  }
);

const weatherSlice = createSlice({
  name: 'weather',
  initialState: {
    data: null as HourlyWeatherResponse | null,
    loading: false,
    error: null as string | null,
    suggestions: [] as AutocompleteItem[]
  },
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchHourlyWeather.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchHourlyWeather.fulfilled, (state, action) => {
        state.loading = false;
        if (action.payload.cityFound) {
          state.data = action.payload.weather;
        } else {
          state.suggestions = action.payload.suggestions;
          state.error = action.payload.message;
        }
      })
      .addCase(fetchHourlyWeather.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Unknown error';
      });
  }
});

export default weatherSlice.reducer;
```

This comprehensive guide provides everything needed for frontend integration with your new hourly weather API! 🌤️ 