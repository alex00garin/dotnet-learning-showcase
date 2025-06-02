# .NET Learning Showcase

This project combines multiple .NET learning examples (Level1, Level2, etc.) into a single deployable application.

## Structure

The application is organized with different route prefixes for each level:

- `/level1` - Basic Weather API (fetches weather data from external API)
- `/level2` - Weather CRUD (uses Postgres database for storing weather data)

## Key Endpoints

### Level 1
- GET `/level1/weatherforecast?city={cityName}` - Get current weather for a city

### Level 2
- GET `/level2/weatherforecast/history/{city}` - Get stored weather history for a city
- POST `/level2/weatherforecast/save` - Save weather forecasts to database
- PUT `/level2/weatherforecast/update/{id}` - Update a weather forecast summary
- DELETE `/level2/weatherforecast/delete/{id}` - Delete a weather forecast

## Deployment

The application is configured for deployment to Fly.io:

1. Make sure you have Fly CLI installed: `curl -L https://fly.io/install.sh | sh`
2. Login to Fly.io: `fly auth login`
3. Deploy using the script: `./deploy.sh`

## Local Development

To run the application locally:

```bash
cd DotnetLearningShowcase
dotnet run
```

## Configuration

Database connection strings and other configuration settings can be modified in `appsettings.json` or provided as environment variables during deployment. 