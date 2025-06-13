# Hourly Weather Forecast Endpoints

This document describes the new hourly weather forecast functionality added to the .NET Learning Showcase API.

## Overview

The application now supports **hourly weather forecasts** using the Open-Meteo API. These endpoints provide detailed weather data for the current day (24 hours) with comprehensive information for each hour.

## Endpoints

### 1. Basic Hourly Weather Forecast

```
GET /level1/weatherforecast/hourly?city={cityName}
```

**Description**: Get hourly weather forecast for today for a specific city.

**Parameters**:
- `city` (optional): City name (defaults to "Berlin")

**Example**:
```bash
curl "http://localhost:5249/level1/weatherforecast/hourly?city=Berlin"
```

### 2. Smart Hourly Weather Forecast

```
GET /level1/weatherforecast/hourly/smart?city={cityName}
```

**Description**: Get hourly weather forecast with intelligent city suggestions when city is not found.

**Parameters**:
- `city` (optional): City name (defaults to "Berlin")

**Example**:
```bash
curl "http://localhost:5249/level1/weatherforecast/hourly/smart?city=Lond"
```

## Response Format

### Successful Response

```json
{
  "location": "Berlin, Germany",
  "hourlyForecasts": [
    {
      "time": "2025-06-13T00:00:00",
      "temperatureC": 15.6,
      "precipitation": 0,
      "weatherCode": 0,
      "windSpeed": 13.2,
      "humidity": 46,
      "apparentTemperature": 12.2,
      "temperatureF": 60.08,
      "weatherDescription": "Clear sky"
    },
    // ... 23 more hours
  ]
}
```

### Smart Endpoint Success Response

```json
{
  "weather": {
    "location": "London, United Kingdom",
    "hourlyForecasts": [/* ... */]
  },
  "cityFound": true,
  "suggestions": null
}
```

### Smart Endpoint Error Response

```json
{
  "message": "City 'NotACity' not found.",
  "cityFound": false,
  "suggestions": [],
  "hint": "Try one of the suggested cities or use /level3/cities/autocomplete for more options"
}
```

## Data Fields

Each hourly forecast contains:

| Field | Type | Description |
|-------|------|-------------|
| `time` | string | ISO 8601 datetime for the forecast hour |
| `temperatureC` | number | Temperature in Celsius |
| `temperatureF` | number | Temperature in Fahrenheit (calculated) |
| `precipitation` | number | Precipitation amount in millimeters |
| `weatherCode` | integer | WMO weather code |
| `weatherDescription` | string | Human-readable weather description |
| `windSpeed` | number | Wind speed in km/h |
| `humidity` | number | Relative humidity percentage |
| `apparentTemperature` | number | Feels-like temperature in Celsius |

## Weather Descriptions

The API converts WMO weather codes to human-readable descriptions:

| Code | Description |
|------|-------------|
| 0 | Clear sky |
| 1 | Mainly clear |
| 2 | Partly cloudy |
| 3 | Overcast |
| 45, 48 | Foggy |
| 51, 53, 55 | Drizzle |
| 61, 63, 65 | Rain |
| 71, 73, 75 | Snow |
| 80, 81, 82 | Rain showers |
| 95 | Thunderstorm |

## Features

✅ **Real-time Data**: Uses Open-Meteo API for up-to-date weather information
✅ **24-Hour Coverage**: Provides hourly forecasts for the current day
✅ **Rich Data**: Temperature, precipitation, wind, humidity, and more
✅ **Smart Geocoding**: Automatically resolves city names to coordinates
✅ **Error Handling**: Graceful handling of invalid cities with suggestions
✅ **Multiple Units**: Both Celsius and Fahrenheit temperatures
✅ **Integration**: Works with existing autocomplete system

## Open-Meteo API Integration

The implementation uses the Open-Meteo Forecast API with these parameters:

```
https://api.open-meteo.com/v1/forecast
?latitude={lat}&longitude={lon}
&hourly=temperature_2m,precipitation,weather_code,wind_speed_10m,relative_humidity_2m,apparent_temperature
&timezone=auto
&forecast_days=1
```

### Key Parameters

- `hourly`: Specifies which hourly variables to fetch
- `forecast_days=1`: Limits to today only (24 hours)
- `timezone=auto`: Automatically resolves timezone for the location

## Testing

Use the provided test script to verify functionality:

```bash
./test-hourly-weather.sh
```

This script tests:
- Basic hourly forecasts for multiple cities
- Smart endpoint with invalid cities
- Partial city name resolution
- Global city coverage

## Architecture

The hourly weather functionality integrates with the existing clean architecture:

1. **Models**: `HourlyWeatherForecast`, `HourlyWeatherResponse`
2. **Services**: Extended `IWeatherService` and `IWeatherApiService`
3. **Data Source**: Open-Meteo API via `WeatherApiService`
4. **Endpoints**: Level 1 endpoints with basic and smart versions
5. **Integration**: Uses existing `GeocodingService` and `AutocompleteService`

## Compatibility

- ✅ Fully backward compatible with existing endpoints
- ✅ Uses same dependency injection configuration
- ✅ Follows existing error handling patterns
- ✅ Integrates with autocomplete system seamlessly 