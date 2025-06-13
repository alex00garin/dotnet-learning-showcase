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

### HourlyWeatherComparisonResponse
```typescript
interface HourlyWeatherComparisonResponse {
  location: string;              // "Berlin, Germany"
  todayForecasts: HourlyWeatherForecast[];      // 24 hours for today
  yesterdayForecasts: HourlyWeatherForecast[];  // 24 hours for yesterday
  comparison: WeatherComparison;                // Temperature comparison data
}
```

### WeatherComparison
```typescript
interface WeatherComparison {
  todayAverageTemp: number;      // 23.1 (°C)
  yesterdayAverageTemp: number;  // 21.4 (°C)
  temperatureDifference: number; // 1.7 (today - yesterday)
  comparisonText: string;        // "Today is warmer than yesterday (+1.7°C)"
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
  weather: HourlyWeatherComparisonResponse;
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
// Fetch hourly weather comparison for Berlin
const response = await fetch('/level1/weatherforecast/hourly?city=Berlin');
const data = await response.json();

// Example response:
{
  "location": "Berlin, Germany",
  "todayForecasts": [
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
    // ... 23 more hours for today
  ],
  "yesterdayForecasts": [
    {
      "time": "2024-01-14T14:00:00",
      "temperatureC": 19.8,
      "temperatureF": 67.6,
      "precipitation": 0.2,
      "weatherCode": 2,
      "weatherDescription": "Partly cloudy",
      "windSpeed": 12.1,
      "humidity": 68.0,
      "apparentTemperature": 18.5
    }
    // ... 23 more hours for yesterday
  ],
  "comparison": {
    "todayAverageTemp": 23.1,
    "yesterdayAverageTemp": 21.4,
    "temperatureDifference": 1.7,
    "comparisonText": "Today is warmer than yesterday (+1.7°C)"
  }
}
```

### Smart Forecast (Success)
```javascript
const response = await fetch('/level1/weatherforecast/hourly/smart?city=Berl');
const data = await response.json();

// Response includes weather comparison data + metadata
{
  "weather": {
    "location": "Berlin, Germany",
    "todayForecasts": [/* 24 hours of today's data */],
    "yesterdayForecasts": [/* 24 hours of yesterday's data */],
    "comparison": {
      "todayAverageTemp": 23.1,
      "yesterdayAverageTemp": 21.4,
      "temperatureDifference": 1.7,
      "comparisonText": "Today is warmer than yesterday (+1.7°C)"
    }
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

### Weather Comparison Display
```tsx
import React, { useState, useEffect } from 'react';

interface WeatherDisplayProps {
  city: string;
}

const HourlyWeatherComparisonDisplay: React.FC<WeatherDisplayProps> = ({ city }) => {
  const [weather, setWeather] = useState<HourlyWeatherComparisonResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'today' | 'yesterday' | 'comparison'>('today');

  useEffect(() => {
    const fetchWeather = async () => {
      setLoading(true);
      setError(null);
      
      try {
        const response = await fetch(`/level1/weatherforecast/hourly?city=${encodeURIComponent(city)}`);
        
        if (!response.ok) {
          throw new Error(`City '${city}' not found`);
        }
        
        const data: HourlyWeatherComparisonResponse = await response.json();
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

  const getCurrentForecasts = () => {
    switch (activeTab) {
      case 'today':
        return weather.todayForecasts;
      case 'yesterday':
        return weather.yesterdayForecasts;
      default:
        return [];
    }
  };

  return (
    <div className="weather-display">
      <h2>📍 {weather.location}</h2>
      
      {/* Temperature Comparison Summary */}
      <div className="comparison-summary">
        <h3>🌡️ Temperature Comparison</h3>
        <p className="comparison-text">{weather.comparison.comparisonText}</p>
        <div className="temp-stats">
          <span className="today-temp">Today: {weather.comparison.todayAverageTemp.toFixed(1)}°C</span>
          <span className="yesterday-temp">Yesterday: {weather.comparison.yesterdayAverageTemp.toFixed(1)}°C</span>
          <span className={`temp-diff ${weather.comparison.temperatureDifference >= 0 ? 'warmer' : 'colder'}`}>
            {weather.comparison.temperatureDifference >= 0 ? '+' : ''}{weather.comparison.temperatureDifference.toFixed(1)}°C
          </span>
        </div>
      </div>

      {/* Tab Navigation */}
      <div className="tab-navigation">
        <button 
          className={activeTab === 'today' ? 'active' : ''} 
          onClick={() => setActiveTab('today')}
        >
          Today
        </button>
        <button 
          className={activeTab === 'yesterday' ? 'active' : ''} 
          onClick={() => setActiveTab('yesterday')}
        >
          Yesterday
        </button>
        <button 
          className={activeTab === 'comparison' ? 'active' : ''} 
          onClick={() => setActiveTab('comparison')}
        >
          Side-by-Side
        </button>
      </div>

      {/* Hourly Display */}
      {activeTab !== 'comparison' ? (
        <div className="hourly-grid">
          {getCurrentForecasts().map((forecast, index) => (
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
      ) : (
        <div className="comparison-grid">
          {weather.todayForecasts.map((todayForecast, index) => {
            const yesterdayForecast = weather.yesterdayForecasts[index];
            const tempDiff = todayForecast.temperatureC - yesterdayForecast.temperatureC;
            
            return (
              <div key={index} className="comparison-hour">
                <div className="time">{new Date(todayForecast.time).toLocaleTimeString('en-US', { hour: '2-digit' })}</div>
                <div className="day-comparison">
                  <div className="today">
                    <span className="label">Today</span>
                    <span className="temp">{Math.round(todayForecast.temperatureC)}°C</span>
                  </div>
                  <div className="yesterday">
                    <span className="label">Yesterday</span>
                    <span className="temp">{Math.round(yesterdayForecast.temperatureC)}°C</span>
                  </div>
                  <div className={`temp-diff ${tempDiff >= 0 ? 'warmer' : 'colder'}`}>
                    {tempDiff >= 0 ? '+' : ''}{tempDiff.toFixed(1)}°C
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
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
  const [weather, setWeather] = useState<HourlyWeatherComparisonResponse | null>(null);
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

      {weather && <HourlyWeatherComparisonDisplay city={weather.location} />}
    </div>
  );
};
```

### Weather Comparison Chart Component
```tsx
import React from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

interface WeatherComparisonChartProps {
  comparisonData: HourlyWeatherComparisonResponse;
}

const WeatherComparisonChart: React.FC<WeatherComparisonChartProps> = ({ comparisonData }) => {
  const chartData = comparisonData.todayForecasts.map((todayForecast, index) => {
    const yesterdayForecast = comparisonData.yesterdayForecasts[index];
    
    return {
      time: new Date(todayForecast.time).toLocaleTimeString('en-US', { hour: '2-digit' }),
      todayTemp: todayForecast.temperatureC,
      yesterdayTemp: yesterdayForecast.temperatureC,
      tempDifference: todayForecast.temperatureC - yesterdayForecast.temperatureC,
      todayPrecip: todayForecast.precipitation,
      yesterdayPrecip: yesterdayForecast.precipitation
    };
  });

  return (
    <div className="weather-chart">
      <h3>📈 Temperature Comparison: Today vs Yesterday</h3>
      <div className="chart-summary">
        <p>{comparisonData.comparison.comparisonText}</p>
      </div>
      
      <ResponsiveContainer width="100%" height={400}>
        <LineChart data={chartData}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="time" />
          <YAxis />
          <Tooltip 
            formatter={(value, name) => [
              `${typeof value === 'number' ? value.toFixed(1) : value}${name.includes('Temp') ? '°C' : name.includes('Precip') ? 'mm' : '°C'}`,
              name.replace(/([A-Z])/g, ' $1').replace(/^./, str => str.toUpperCase())
            ]}
          />
          <Line 
            type="monotone" 
            dataKey="todayTemp" 
            stroke="#ff6b6b" 
            strokeWidth={3}
            name="Today Temperature"
            dot={{ fill: '#ff6b6b', strokeWidth: 2, r: 3 }}
          />
          <Line 
            type="monotone" 
            dataKey="yesterdayTemp" 
            stroke="#4ecdc4" 
            strokeWidth={2}
            strokeDasharray="5 5"
            name="Yesterday Temperature"
            dot={{ fill: '#4ecdc4', strokeWidth: 2, r: 3 }}
          />
        </LineChart>
      </ResponsiveContainer>
      
      <div className="chart-legend">
        <div className="legend-item">
          <span className="legend-line today"></span>
          <span>Today ({comparisonData.comparison.todayAverageTemp.toFixed(1)}°C avg)</span>
        </div>
        <div className="legend-item">
          <span className="legend-line yesterday"></span>
          <span>Yesterday ({comparisonData.comparison.yesterdayAverageTemp.toFixed(1)}°C avg)</span>
        </div>
      </div>
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
        return { 
          success: true, 
          weather: data.weather,
          comparison: data.weather.comparison 
        };
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

### Temperature Comparison Utils
```typescript
const temperatureComparisonUtils = {
  getComparisonIcon: (difference: number): string => {
    if (difference > 2) return '🔥'; // Much warmer
    if (difference > 0.5) return '🌡️↗️'; // Warmer
    if (difference < -2) return '🧊'; // Much colder
    if (difference < -0.5) return '🌡️↘️'; // Colder
    return '🌡️'; // Similar
  },
  
  getComparisonColor: (difference: number): string => {
    if (difference > 2) return '#ff4444'; // Hot red
    if (difference > 0.5) return '#ff8844'; // Warm orange
    if (difference < -2) return '#4488ff'; // Cold blue
    if (difference < -0.5) return '#44aaff'; // Cool light blue
    return '#666666'; // Neutral gray
  },
  
  formatComparison: (todayTemp: number, yesterdayTemp: number): string => {
    const diff = todayTemp - yesterdayTemp;
    const absDiff = Math.abs(diff);
    
    if (absDiff < 0.5) {
      return `Similar temperatures (${diff > 0 ? '+' : ''}${diff.toFixed(1)}°C)`;
    }
    
    const comparison = diff > 0 ? 'warmer' : 'colder';
    const intensity = absDiff > 2 ? 'much ' : '';
    
    return `${intensity}${comparison} than yesterday (${diff > 0 ? '+' : ''}${diff.toFixed(1)}°C)`;
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

.comparison-summary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 1rem;
  border-radius: 8px;
  text-align: center;
}

.temp-stats {
  display: flex;
  justify-content: space-around;
  margin-top: 0.5rem;
}

.temp-diff.warmer {
  color: #ff6b6b;
}

.temp-diff.colder {
  color: #4ecdc4;
}

.tab-navigation {
  display: flex;
  gap: 0.5rem;
  margin: 1rem 0;
}

.tab-navigation button {
  flex: 1;
  padding: 0.75rem;
  border: none;
  background: #f0f0f0;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
}

.tab-navigation button.active {
  background: #667eea;
  color: white;
}

.hourly-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(80px, 1fr));
  gap: 0.5rem;
  overflow-x: auto;
  padding-bottom: 1rem;
}

.comparison-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: 0.5rem;
  overflow-x: auto;
}

.comparison-hour {
  background: #f9f9f9;
  border-radius: 6px;
  padding: 0.5rem;
  text-align: center;
}

.day-comparison {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.chart-legend {
  display: flex;
  justify-content: center;
  gap: 2rem;
  margin-top: 1rem;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.legend-line {
  width: 20px;
  height: 3px;
  border-radius: 2px;
}

.legend-line.today {
  background: #ff6b6b;
}

.legend-line.yesterday {
  background: #4ecdc4;
  background-image: repeating-linear-gradient(
    45deg,
    transparent,
    transparent 2px,
    #fff 2px,
    #fff 4px
  );
}

@media (max-width: 768px) {
  .hourly-grid, .comparison-grid {
    grid-template-columns: repeat(24, minmax(60px, 1fr));
    scroll-snap-type: x mandatory;
  }
  
  .hour-card, .comparison-hour {
    scroll-snap-align: start;
  }

  .temp-stats {
    flex-direction: column;
    gap: 0.25rem;
  }

  .chart-legend {
    flex-direction: column;
    gap: 0.5rem;
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
  const [weather, setWeather] = useState<HourlyWeatherComparisonResponse | null>(null);
  
  useEffect(() => {
    const fetchWeather = async () => {
      try {
        const response = await fetch(`/level1/weatherforecast/hourly?city=${encodeURIComponent(city)}`);
        if (response.ok) {
          const data: HourlyWeatherComparisonResponse = await response.json();
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
    data: null as HourlyWeatherComparisonResponse | null,
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

## 🎯 Summary

This updated hourly weather API now provides:

✅ **Today's 24-hour forecast** - Complete hourly data for today  
✅ **Yesterday's 24-hour data** - Historical comparison data  
✅ **Temperature comparison** - Average temps and difference analysis  
✅ **Smart city search** - Autocomplete suggestions for invalid cities  
✅ **Rich data format** - Temperature, precipitation, weather codes, wind, humidity  

### 🔄 Key Changes from Previous Version

1. **Response Format**: Now returns `HourlyWeatherComparisonResponse` instead of `HourlyWeatherResponse`
2. **Dual Data Sets**: Includes both `todayForecasts` and `yesterdayForecasts` arrays
3. **Comparison Metrics**: New `comparison` object with temperature analysis
4. **Enhanced Frontend Components**: Tab-based UI for today/yesterday/side-by-side views
5. **Improved Charts**: Dual-line charts showing temperature trends over time

### 🌡️ Use Cases

- **Daily Planning**: "Should I dress warmer than yesterday?"
- **Trend Analysis**: "Is today unusually warm/cold for this time of year?"
- **Quick Comparison**: "How does today compare to yesterday?"
- **Data Visualization**: Side-by-side hourly temperature comparisons

This comprehensive guide provides everything needed for frontend integration with your enhanced hourly weather comparison API! 🌤️📊 