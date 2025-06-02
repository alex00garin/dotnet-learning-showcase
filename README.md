# .NET Learning Showcase

A collection of .NET projects demonstrating various concepts and best practices.

## Projects

### Level 1 - Basic Web API
A RESTful Web API project with:
- Weather forecast endpoint using real weather data
- Integration tests
- Swagger documentation
- Dependency injection
- Error handling

### Level 2 - Intermediate Web API
An enhanced version of the basic API with:
- Dynamic city search
- Real-time weather data
- Advanced error handling
- Comprehensive testing
- CRUD operations for weather data
- Database storage
- Deployed to Fly.io

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/dotnet-learning-showcase.git
   cd dotnet-learning-showcase
   ```

2. Run the tests:
   ```bash
   cd Level1/BasicWebApi.Tests
   dotnet test
   ```

3. Run the API:
   ```bash
   cd Level1/BasicWebApi
   dotnet run
   ```

4. Access the Swagger UI:
   ```
   http://localhost:5245/swagger
   ```

## API Endpoints

### Weather Forecast
- `GET /weatherforecast?city={cityName}`
  - Returns weather forecast for the specified city
  - Default city: Berlin
  - Example: `/weatherforecast?city=London`

### Level 2 Weather CRUD API
- `GET /weatherforecast/history/{cityName}`
  - Returns saved weather forecasts for the specified city
- `POST /weatherforecast/save`
  - Saves a new weather forecast
- `GET /health`
  - Health check endpoint

## Deployment to Fly.io

The Level 2 API is deployed to Fly.io and accessible at:
```
https://dotnet-weather-crud.fly.dev/
```

### Deploying Updates

To deploy updates to an existing level:

```bash
./deploy-to-fly.sh 2 dotnet-weather-crud
```

This script will build and deploy the application to Fly.io.

### Adding New Levels

To add a new level (e.g., Level 3):

1. Create the project structure:
   ```bash
   mkdir -p Level3/Level3.YourNewApp
   ```

2. Initialize a new Fly.io application:
   ```bash
   cd Level3
   fly launch --name your-level3-app-name
   ```

3. Create a Dockerfile in the Level3 directory or update the existing one to point to your new application.

4. Deploy your new level:
   ```bash
   ./deploy-to-fly.sh 3 your-level3-app-name
   ```

### Deployment Prerequisites

- Install Fly.io CLI: `brew install flyctl` (macOS) or see [Fly.io docs](https://fly.io/docs/hands-on/install-flyctl/)
- Login to Fly.io: `fly auth login`
- Set up your database connection string as an environment variable in Fly.io:
  ```bash
  fly secrets set DATABASE_URL="your-connection-string" --app your-app-name
  ```

## Technologies Used

- .NET 8
- ASP.NET Core
- xUnit
- FluentAssertions
- Open-Meteo API
- PostgreSQL (via Supabase)
- Docker
- Fly.io for hosting

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.