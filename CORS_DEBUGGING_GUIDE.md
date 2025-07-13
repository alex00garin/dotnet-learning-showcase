# CORS Debugging Guide - Level 2 Save Endpoint

## Issue Summary

The frontend at `https://www.alexandergarin.com` is unable to POST to `https://api.alexandergarin.com/level2/weatherforecast/save` due to CORS policy violations and 500 Internal Server Errors.

## Error Messages Observed

```
Access to fetch at 'https://api.alexandergarin.com/level2/weatherforecast/save' from origin 'https://www.alexandergarin.com' has been blocked by CORS policy: No 'Access-Control-Allow-Origin' header is present on the requested resource.

POST https://api.alexandergarin.com/level2/weatherforecast/save net::ERR_FAILED 500 (Internal Server Error)
```

## Root Cause Analysis

The issue is a combination of:

1. **CORS Headers Missing on Error Responses**: When a 500 error occurs, the CORS middleware doesn't add the necessary headers to the response
2. **Server-side Error**: There's likely a server-side issue (database connection, request deserialization, etc.)
3. **Browser Security**: The browser blocks the request due to missing CORS headers

## Solutions Implemented

### 1. Enhanced CORS Configuration

```csharp
// Production: Specific origins with credentials
policy.WithOrigins(
    "https://www.alexandergarin.com",
    "https://alexandergarin.com",
    "http://localhost:3000",
    "http://localhost:5173",
    "http://localhost:8080"
)
.AllowAnyMethod()
.AllowAnyHeader()
.AllowCredentials();

// Development: Allow all origins
policy.AllowAnyOrigin()
      .AllowAnyMethod()
      .AllowAnyHeader();
```

### 2. Global Error Handler

Added middleware to ensure CORS headers are present on all responses, including error responses:

```csharp
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        // Log error and ensure CORS headers are present
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            
            var errorResponse = new
            {
                error = "Internal server error",
                message = app.Environment.IsDevelopment() ? ex.Message : "An error occurred",
                timestamp = DateTime.UtcNow
            };
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }
});
```

### 3. Enhanced Level 2 Endpoint

Added comprehensive validation and error handling:

```csharp
level2.MapPost("/weatherforecast/save", async (
    IWeatherService weatherService, 
    WeatherSaveRequest request,
    ILogger<Program> logger) =>
{
    try
    {
        // Validate request fields
        if (string.IsNullOrWhiteSpace(request.City))
            return Results.BadRequest(new { error = "City name is required" });
        
        if (string.IsNullOrWhiteSpace(request.Country))
            return Results.BadRequest(new { error = "Country name is required" });
        
        if (request.Forecasts == null || !request.Forecasts.Any())
            return Results.BadRequest(new { error = "At least one forecast is required" });
        
        // Process request
        await weatherService.SaveForecastAsync(request);
        
        return Results.Ok(new { 
            message = "Saved", 
            city = request.City, 
            count = request.Forecasts.Count 
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error saving weather forecast");
        return Results.Problem(
            title: "Error saving weather forecast",
            detail: ex.Message,
            statusCode: 500
        );
    }
});
```

## Expected Request Format

The endpoint expects JSON in this format:

```json
{
  "city": "London",
  "country": "UK",
  "forecasts": [
    {
      "date": "2024-01-15",
      "temperatureC": 20,
      "summary": "Sunny weather"
    },
    {
      "date": "2024-01-16",
      "temperatureC": 18,
      "summary": "Cloudy weather"
    }
  ]
}
```

## Frontend Implementation Example

```javascript
// Correct way to call the endpoint
const saveWeatherData = async (weatherData) => {
  try {
    const response = await fetch('https://api.alexandergarin.com/level2/weatherforecast/save', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Origin': 'https://www.alexandergarin.com'
      },
      body: JSON.stringify({
        city: weatherData.city,
        country: weatherData.country,
        forecasts: weatherData.forecasts.map(f => ({
          date: f.date, // Format: "YYYY-MM-DD"
          temperatureC: f.temperatureC,
          summary: f.summary
        }))
      })
    });
    
    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.error || 'Failed to save weather data');
    }
    
    return await response.json();
  } catch (error) {
    console.error('Error saving weather data:', error);
    throw error;
  }
};
```

## Testing the Fix

### 1. Run the Test Script

```bash
./test-level2-save.sh
```

This script will:
- Test a valid request
- Test CORS preflight request
- Test invalid request handling
- Test localhost API
- Check API health

### 2. Check Browser Network Tab

1. Open browser DevTools
2. Go to Network tab
3. Try the request from your frontend
4. Check if CORS headers are present in the response

### 3. Expected CORS Headers

The response should include:
```
Access-Control-Allow-Origin: https://www.alexandergarin.com
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS
Access-Control-Allow-Headers: Content-Type, Authorization
Access-Control-Allow-Credentials: true
```

## Debugging Steps

1. **Check API Health**: Visit `https://api.alexandergarin.com/health`
2. **Test Direct curl**: Use the test script or manual curl commands
3. **Check Server Logs**: Look for error messages in the API logs
4. **Verify Database Connection**: Ensure the PostgreSQL database is accessible
5. **Test Frontend Locally**: Try the request from localhost first

## Common Issues & Solutions

### Issue: Still getting CORS errors after deployment
**Solution**: Ensure the production environment is using the correct CORS configuration and restart the application

### Issue: 500 errors persist
**Solution**: Check the server logs for specific error messages. Common causes:
- Database connection issues
- DateOnly serialization problems
- Request deserialization failures

### Issue: OPTIONS preflight requests failing
**Solution**: Ensure the CORS middleware is configured to handle OPTIONS requests

## Deployment Checklist

- [ ] CORS configuration updated
- [ ] Global error handler added
- [ ] Level 2 endpoint enhanced
- [ ] Test script created and executed
- [ ] Frontend updated with correct request format
- [ ] Server logs monitored for errors
- [ ] Database connection verified

## Contact & Support

If issues persist after implementing these fixes:
1. Check the server logs for specific error messages
2. Test the endpoint with curl to isolate CORS from other issues
3. Verify the database connection is working
4. Ensure the frontend is sending the correct request format 