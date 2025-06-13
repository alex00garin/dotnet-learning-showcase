# API Quick Reference - Hourly Weather

## 🚀 Endpoints Summary

### Hourly Weather Endpoints
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/level1/weatherforecast/hourly` | GET | Basic hourly weather (24 hours) |
| `/level1/weatherforecast/hourly/smart` | GET | Smart hourly with autocomplete fallback |
| `/level3/cities/autocomplete` | GET | City name autocomplete |

### Parameters
- `city` (optional): City name, defaults to "Berlin"
- `query` (required for autocomplete): Partial city name (min 2 chars)

## 📊 Response Formats

### Successful Hourly Weather Response
```json
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

### Smart Endpoint Success
```json
{
  "weather": { /* HourlyWeatherResponse */ },
  "cityFound": true
}
```

### Smart Endpoint Error (404)
```json
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
  ],
  "hint": "Try using autocomplete: /level3/cities/autocomplete?query=NotACity"
}
```

### Autocomplete Response
```json
[
  {
    "name": "London",
    "fullName": "London, United Kingdom",
    "country": "United Kingdom",
    "latitude": 51.50853,
    "longitude": -0.12574,
    "relevanceScore": 0.98
  }
]
```

## 🌤️ Weather Codes
| Code | Description | Icon |
|------|-------------|------|
| 0 | Clear sky | ☀️ |
| 1 | Mainly clear | 🌤️ |
| 2 | Partly cloudy | ⛅ |
| 3 | Overcast | ☁️ |
| 45, 48 | Foggy | 🌫️ |
| 51, 53, 55 | Drizzle | 🌦️ |
| 61, 63, 65 | Rain | 🌧️ |
| 71, 73, 75 | Snow | 🌨️ |
| 80, 81, 82 | Rain showers | 🌦️ |
| 95 | Thunderstorm | ⛈️ |

## 🔗 Quick Examples

### Fetch Berlin Weather
```bash
curl "http://localhost:5249/level1/weatherforecast/hourly?city=Berlin"
```

### Search with Autocomplete
```bash
curl "http://localhost:5249/level3/cities/autocomplete?query=Lond"
```

### Smart Search with Fallback
```bash
curl "http://localhost:5249/level1/weatherforecast/hourly/smart?city=Berl"
```

## ✅ Status Codes
- `200`: Success
- `404`: City not found (smart endpoints return suggestions)
- `500`: Server error 