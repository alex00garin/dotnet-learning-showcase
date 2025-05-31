# Front-End API Documentation

This document describes the APIs for both **Level 1 (Basic Forecast)** and **Level 2 (Advanced Weather & Logs)**.

---

## Level 1: Basic Forecast API

Base URL: `http://localhost:5245`

### Get Weather Forecast

- **Request**: `GET /weatherforecast?city={city}`
- **Example**:
  ```bash
  curl -i http://localhost:5245/weatherforecast?city=London
  ```
- **Response**: `200 OK` + JSON object:
  ```json
  {
    "Location": "London, UK",
    "Forecasts": [
      { "Date": "2024-03-15", "TemperatureC": 18, "Summary": "Mild with occasional clouds" }
    ]
  }
  ```
- **Errors**:
  - `404 Not Found` if city not found
  - `400 Bad Request` if missing or invalid `city` parameter

---

## Level 2: Advanced API

Base URL: `http://localhost:5260/api`

## Models

### WeatherRecord
```json
{
  "id": 1,
  "location": "London",
  "timestamp": "2024-03-15T10:00:00Z",
  "temperature": 18.5,
  "humidity": 65,
  "windSpeed": 12.3,
  "windDirection": "NW",
  "pressure": 1013.2,
  "conditions": "Partly Cloudy"
}
```

| Field         | Type    | Description                             |
|---------------|---------|-----------------------------------------|
| id            | integer | Unique record identifier                |
| location      | string  | City or location name                   |
| timestamp     | string  | UTC date-time (ISO 8601)                |
| temperature   | number  | Celsius temperature                     |
| humidity      | number  | Relative humidity (%)                   |
| windSpeed     | number  | Wind speed in km/h (or configured unit) |
| windDirection | string  | Wind direction (e.g. "NW")            |
| pressure      | number  | Atmospheric pressure (hPa)              |
| conditions    | string  | Weather summary                         |

### ForecastLog
```json
{
  "id": 2,
  "city": "London",
  "requestedAt": "2024-03-15T09:00:00Z",
  "summary": "Mild with occasional clouds",
  "temperatureC": 18.5,
  "temperatureF": 65.3
}
```

| Field         | Type    | Description                             |
|---------------|---------|-----------------------------------------|
| id            | integer | Unique log identifier                   |
| city          | string  | City name                               |
| requestedAt   | string  | UTC date-time when forecast was logged  |
| summary       | string  | Forecast summary text                   |
| temperatureC  | number  | Forecast temperature in Celsius         |
| temperatureF  | number  | Computed °F                             |

---

## Endpoints

### Weather Records

#### 1) Get history for a location

- **Request**: `GET /weather/{location}`
- **Example**:
  ```bash
  curl http://localhost:5260/api/weather/London
  ```
- **Response**: `200 OK` + JSON array of `WeatherRecord`
- **Not Found**: `404 Not Found` if no records exist

#### 2) Get current weather

- **Request**: `GET /weather/{location}/current`
- **Example**:
  ```bash
  curl http://localhost:5260/api/weather/London/current
  ```
- **Response**: `200 OK` + single `WeatherRecord`
- **Not Found**: `404 Not Found` if no records exist

#### 3) Create a new record

- **Request**: `POST /weather`
- **Headers**: `Content-Type: application/json`
- **Body**:
  ```json
  {
    "location": "Paris",
    "temperature": 22.5,
    "humidity": 70,
    "windSpeed": 10.0,
    "windDirection": "NE",
    "pressure": 1013.2,
    "conditions": "Sunny"
  }
  ```
- **Response**: `201 Created` + created `WeatherRecord`
- **Error**: `400 Bad Request` if `location` is missing or empty


### Forecast Logs

#### 1) List all logs

- **Request**: `GET /forecastlogs`
- **Example**:
  ```bash
  curl http://localhost:5260/api/forecastlogs
  ```
- **Response**: `200 OK` + JSON array of `ForecastLog`

#### 2) Get log by ID

- **Request**: `GET /forecastlogs/{id}`
- **Example**:
  ```bash
  curl http://localhost:5260/api/forecastlogs/1
  ```
- **Response**: `200 OK` + single `ForecastLog`
- **Not Found**: `404 Not Found`

#### 3) Get logs by city

- **Request**: `GET /forecastlogs/city/{city}`
- **Example**:
  ```bash
  curl http://localhost:5260/api/forecastlogs/city/London
  ```
- **Response**: `200 OK` + JSON array of `ForecastLog`
- **Not Found**: `404 Not Found` if none

#### 4) Create a new log

- **Request**: `POST /forecastlogs`
- **Headers**: `Content-Type: application/json`
- **Body**:
  ```json
  {
    "city": "Berlin",
    "summary": "Partly cloudy with light rain",
    "temperatureC": 18.5
  }
  ```
- **Response**: `201 Created` + created `ForecastLog` (with `id`, `requestedAt`)

#### 5) Update a log

- **Request**: `PUT /forecastlogs/{id}`
- **Headers**: `Content-Type: application/json`
- **Body** (all fields required):
  ```json
  {
    "id": 1,
    "city": "London",
    "requestedAt": "2024-03-15T09:00:00Z",
    "summary": "Updated forecast",
    "temperatureC": 20.0
  }
  ```
- **Response**: `204 No Content`
- **Errors**:
  - `400 Bad Request` if `id` mismatch
  - `404 Not Found` if log does not exist

#### 6) Delete a log

- **Request**: `DELETE /forecastlogs/{id}`
- **Example**:
  ```bash
  curl -X DELETE http://localhost:5260/api/forecastlogs/1
  ```
- **Response**: `204 No Content`
- **Not Found**: `404 Not Found`

---

### Notes

- All date-time fields use UTC ISO 8601 format.
- Ensure `Content-Type: application/json` on POST/PUT.
- Example host/port can be changed via `launchSettings.json`.

---
Generated on project version .NET 9 / PostgreSQL backend. FE can also explore Swagger UI at `http://localhost:5260/swagger` for interactive testing.

---

## Integration: Saving Level 1 Forecasts into Level 2 Logs

After fetching the 7-day forecast from the **Level 1** API, you can persist each day's summary and temperature using the **Level 2** ForecastLogs CRUD endpoints.

### Workflow

1. **Fetch from Level 1**:
   ```bash
   curl -s "http://localhost:5245/weatherforecast?city=Cardiff" | jq .
   ```
   Response JSON shape:
   ```json
   {
     "location": "Cardiff, United Kingdom",
     "forecasts": [ /* array of 7 items */ ]
   }
   ```

2. **Extract and POST to Level 2**:
   - Endpoint: `POST http://localhost:5260/api/forecastlogs`
   - Payload:
     ```json
     {
       "city": "Cardiff",
       "summary": "Rainy in Cardiff, United Kingdom",
       "temperatureC": 17
     }
     ```
   - Returns `201 Created` with the saved log object.

### JavaScript Example

```js
async function saveForecastsToLogs(city) {
  // 1) Get the 7-day forecast
  const resp = await fetch(`http://localhost:5245/weatherforecast?city=${city}`);
  if (!resp.ok) throw new Error('City not found or API error');
  const { location, forecasts } = await resp.json();
  const cityName = location.split(',')[0];

  // 2) For each forecast day, create a log
  for (const f of forecasts) {
    const logResp = await fetch('http://localhost:5260/api/forecastlogs', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        city: cityName,
        summary: f.summary,
        temperatureC: f.temperatureC
      })
    });
    if (!logResp.ok) {
      console.error('Failed to save log for', f.date, await logResp.text());
    }
  }
}

// Usage:
saveForecastsToLogs('Cardiff');
```

### cURL + jq Pipeline

```bash
curl -s "http://localhost:5245/weatherforecast?city=Cardiff" \
  | jq -c '.forecasts[]' \
  | while read -r f; do
      city="Cardiff"
      summary=$(jq -r '.summary' <<<"$f")
      temp=$(jq -r '.temperatureC' <<<"$f")
      curl -i -X POST http://localhost:5260/api/forecastlogs \
        -H 'Content-Type: application/json' \
        -d "{\"city\":\"$city\",\"summary\":\"$summary\",\"temperatureC\":$temp}";
    done
```

This integration ensures that every forecast from **Level 1** is recorded via **Level 2**'s CRUD system, allowing your front-end to display both real-time and persisted logs. 