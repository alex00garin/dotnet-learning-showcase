# Frontend Integration Guide

## 🚀 Quick Start

### Base URL
```
http://localhost:5249
https://api.alexandergarin.com
```

## 📋 Available Endpoints

### 1. Basic Weather Forecast
**Endpoint:** `GET /level1/weatherforecast?city={cityName}`

Returns current weather conditions for a city.

### 2. Hourly Weather with Yesterday Comparison
**Endpoint:** `GET /level1/weatherforecast/hourly?city={cityName}`

Returns hourly weather forecast for today with full yesterday data and current hour comparison.

### 3. Smart Hourly Weather Forecast (with autocomplete)
```http
GET /level1/weatherforecast/hourly/smart?city={cityName}
```

### 4. City Autocomplete
```http
GET /level3/cities/autocomplete?query={partialCityName}
```

## 📊 Data Structures

### HourlyWeatherComparisonResponse
```json
{
  "location": "Cardiff, United Kingdom",
  "todayForecasts": [
    {
      "time": "2024-01-15T00:00:00",
      "temperatureC": 12.5,
      "temperatureF": 54.5,
      "humidity": 85,
      "precipitation": 0.2,
      "windSpeed": 15.3,
      "weatherDescription": "Light rain",
      "weatherCode": 61
    }
    // ... 23 more hours
  ],
  "yesterdayForecasts": [
    {
      "time": "2024-01-14T00:00:00",
      "temperatureC": 10.2,
      "temperatureF": 50.4,
      "humidity": 78,
      "precipitation": 0.0,
      "windSpeed": 12.1,
      "weatherDescription": "Partly cloudy",
      "weatherCode": 2
    }
    // ... 23 more hours
  ],
  "currentHourComparison": {
    "todayCurrentHour": {
      "time": "2024-01-15T14:00:00",
      "temperatureC": 17.0,
      "temperatureF": 62.6,
      "humidity": 83,
      "precipitation": 0.1,
      "windSpeed": 16.0,
      "weatherDescription": "Drizzle",
      "weatherCode": 51
    },
    "yesterdayCurrentHour": {
      "time": "2024-01-14T14:00:00",
      "temperatureC": 14.5,
      "temperatureF": 58.1,
      "humidity": 75,
      "precipitation": 0.0,
      "windSpeed": 12.0,
      "weatherDescription": "Partly cloudy",
      "weatherCode": 2
    },
    "comparisonText": "Warmer than yesterday (+2.5°C)"
  }
}
```

### HourlyWeatherForecast
```json
{
  "time": "2024-01-15T14:00:00",
  "temperatureC": 17.0,
  "temperatureF": 62.6,
  "humidity": 85,
  "precipitation": 0.1,
  "windSpeed": 16.0,
  "weatherDescription": "Drizzle",
  "weatherCode": 51
}
```

### CurrentHourComparison
```json
{
  "todayCurrentHour": { /* HourlyWeatherForecast object */ },
  "yesterdayCurrentHour": { /* HourlyWeatherForecast object */ },
  "comparisonText": "Warmer than yesterday (+2.5°C)"
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

## 🔗 API Examples & Curl Commands

### Display Current Weather with Yesterday Comparison

```javascript
// Fetch hourly weather data
const response = await fetch('/level1/weatherforecast/hourly?city=Cardiff');
const data = await response.json();

// Display current weather
const current = data.currentHourComparison?.todayCurrentHour;
if (current) {
    document.getElementById('location').textContent = data.location;
    document.getElementById('temperature').textContent = `${Math.round(current.temperatureC)}°`;
    document.getElementById('condition').textContent = current.weatherDescription;
    document.getElementById('feels-like').textContent = `Feels like ${Math.round(current.temperatureC)}°`;
    document.getElementById('humidity').textContent = `${current.humidity}%`;
    document.getElementById('wind').textContent = `${Math.round(current.windSpeed)} km/h`;
    document.getElementById('rain').textContent = `${current.precipitation}mm`;
}

// Display yesterday comparison
if (data.currentHourComparison) {
    const yesterday = data.currentHourComparison.yesterdayCurrentHour;
    const comparisonText = `Yesterday was: ${Math.round(yesterday.temperatureC)}° ${yesterday.weatherDescription}. ${data.currentHourComparison.comparisonText}`;
    document.getElementById('comparison').textContent = comparisonText;
    
    // Example: "Yesterday was: 18° Overcast. Slightly colder than yesterday (-1.2°C)"
}
```

### Display Hourly Chart with Yesterday Overlay

```javascript
// Create temperature comparison chart
const todayTemps = data.todayForecasts.map(f => ({
    time: new Date(f.time).getHours(),
    temp: f.temperatureC
}));

const yesterdayTemps = data.yesterdayForecasts.map(f => ({
    time: new Date(f.time).getHours(),
    temp: f.temperatureC
}));

// Use with Chart.js or similar library
const chartData = {
    labels: Array.from({length: 24}, (_, i) => `${i}:00`),
    datasets: [
        {
            label: 'Today',
            data: todayTemps.map(t => t.temp),
            borderColor: 'rgb(75, 192, 192)',
            tension: 0.1
        },
        {
            label: 'Yesterday',
            data: yesterdayTemps.map(t => t.temp),
            borderColor: 'rgb(255, 99, 132)',
            tension: 0.1,
            borderDash: [5, 5]
        }
    ]
};
```

### Test Current Hour Comparison
```bash
curl "http://localhost:5000/level1/weatherforecast/hourly?city=Cardiff"
```

**Example Response:**
```json
{
  "location": "Cardiff, United Kingdom",
  "todayForecasts": [/* 24 hours of today's data */],
  "yesterdayForecasts": [/* 24 hours of yesterday's data */],
  "currentHourComparison": {
    "todayCurrentHour": {
      "time": "2025-06-14T09:00:00",
      "temperatureC": 16.3,
      "temperatureF": 61.34,
      "precipitation": 0.1,
      "weatherCode": 51,
      "windSpeed": 17.3,
      "humidity": 84,
      "apparentTemperature": 15.0,
      "weatherDescription": "Drizzle"
    },
    "yesterdayCurrentHour": {
      "time": "2025-06-13T09:00:00",
      "temperatureC": 17.5,
      "temperatureF": 63.5,
      "precipitation": 0,
      "weatherCode": 3,
      "windSpeed": 11.5,
      "humidity": 67,
      "apparentTemperature": 16.2,
      "weatherDescription": "Overcast"
    },
    "comparisonText": "Slightly colder than yesterday (-1.2°C)"
  }
}
```

## API Testing

### Expected Response Structure
The API will return:
- `todayForecasts`: Array of 24 hourly forecasts for today
- `yesterdayForecasts`: Array of 24 hourly forecasts for yesterday  
- `currentHourComparison`: Object containing current hour data for both days and comparison text (null if data unavailable)

### Error Handling
- If city not found: Returns 404 with error message
- If yesterday data unavailable: `currentHourComparison` will be null
- Invalid temperature data is filtered out (values < -900°C are considered placeholders)

## Weather Codes Reference
- 0: Clear sky
- 1-3: Mainly clear, partly cloudy, overcast
- 45-48: Fog
- 51-57: Drizzle (light to dense)
- 61-67: Rain (slight to heavy)
- 71-77: Snow fall
- 80-82: Rain showers
- 95-99: Thunderstorm

## Comparison Text Variants

The `comparisonText` field will contain one of these messages based on temperature difference:

- **Much warmer**: `"Much warmer than yesterday (+5.2°C)"` (difference > +5°C)
- **Warmer**: `"Warmer than yesterday (+3.1°C)"` (difference > +2°C)
- **Slightly warmer**: `"Slightly warmer than yesterday (+1.2°C)"` (difference > +0.5°C)
- **Similar**: `"Similar to yesterday (-0.3°C difference)"` (difference between -0.5°C and +0.5°C)
- **Slightly colder**: `"Slightly colder than yesterday (-1.2°C)"` (difference < -0.5°C)
- **Colder**: `"Colder than yesterday (-3.1°C)"` (difference < -2°C)
- **Much colder**: `"Much colder than yesterday (-6.2°C)"` (difference < -5°C)

## Notes
- All temperatures are provided in both Celsius and Fahrenheit
- Times are in ISO 8601 format
- Wind speed is in km/h
- Precipitation is in mm
- Humidity is in percentage (0-100)
- The API uses Open-Meteo's Forecast API with `past_days=1` parameter for recent historical data
- `currentHourComparison` will be `null` if yesterday's data is unavailable for the current hour 