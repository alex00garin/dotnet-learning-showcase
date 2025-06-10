# API Reference - Level 3 Autocomplete System

## 🔗 Endpoints

### City Autocomplete (Recommended)
```
GET /level3/cities/autocomplete?query={text}&limit={number}
```
**Parameters:**
- `query` (required): Search text (min 1 char)
- `limit` (optional): Max results (default: 10, max: 50)

**Response:**
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
```

### Generic Autocomplete
```
POST /level3/autocomplete
Content-Type: application/json
```
**Body:**
```json
{
  "dataSource": "cities",
  "query": "ber",
  "limit": 5
}
```

### Data Sources List
```
GET /level3/datasources
```
**Response:**
```json
[
  {
    "name": "cities",
    "description": "World cities database",
    "itemCount": 154694,
    "isHealthy": true
  }
]
```

### Health Check
```
GET /level3/datasources/{dataSource}/health
```
**Response:**
```json
{
  "dataSource": "cities",
  "isHealthy": true,
  "itemCount": 154694,
  "lastUpdated": "2024-01-15T10:30:00Z"
}
```

## 📊 Data Sources

| Name | Items | Description |
|------|-------|-------------|
| `cities` | 154,694 | World cities database |
| `countries` | 195 | Country names |

## ⚡ Quick Examples

### JavaScript Fetch
```javascript
// City search
const response = await fetch('/level3/cities/autocomplete?query=par&limit=5');
const data = await response.json();
const cities = data.results.map(city => city.label); // Extract labels

// Generic search
const response = await fetch('/level3/autocomplete', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    dataSource: 'cities',
    query: 'par',
    limit: 5
  })
});
```

### cURL Examples
```bash
# City autocomplete
curl "http://localhost:5051/level3/cities/autocomplete?query=ber&limit=5"

# Generic autocomplete
curl -X POST "http://localhost:5051/level3/autocomplete" \
  -H "Content-Type: application/json" \
  -d '{"dataSource": "cities", "query": "ber", "limit": 5}'

# Data sources
curl "http://localhost:5051/level3/datasources"

# Health check
curl "http://localhost:5051/level3/datasources/cities/health"
```

## 🚨 Error Codes

| Code | Description |
|------|-------------|
| 200 | Success |
| 400 | Bad Request (invalid parameters) |
| 500 | Internal Server Error |

## 🎯 Integration Tips

- **Debounce input**: 300ms recommended
- **Limit results**: 5-10 for best UX
- **Handle errors**: Always provide fallbacks
- **Performance**: Local data loads in ~2 seconds, then instant
- **Offline**: ✅ Works offline after initial load 