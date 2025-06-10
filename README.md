# .NET Learning Showcase - Clean Architecture

A showcase project demonstrating .NET 8 best practices with clean architecture, comprehensive testing, and modern deployment strategies. This application has been refactored from a monolithic structure into a maintainable, testable, layered architecture.

## 🚀 Quick Start

```bash
# Clone and run locally
git clone <repository-url>
cd dotnet-learning-showcase

# Restore dependencies
dotnet restore

# Run the application
dotnet run --project DotnetLearningShowcase.csproj

# Run tests
dotnet test Tests/DotnetLearningShowcase.Tests.csproj

# Access the API
curl "http://localhost:5051/level1/weatherforecast?city=Berlin"
curl "http://localhost:5051/health"
```

## 🏗️ Architecture Overview

### Clean Architecture Structure
```
├── Models/                     # Data models and DTOs
│   └── WeatherModels.cs       
├── Services/                   # Business logic layer
│   ├── IWeatherService.cs     # Main service interface
│   ├── WeatherService.cs      # Main service implementation
│   ├── IGeocodingService.cs   # Geocoding service interface
│   ├── GeocodingService.cs    # Geocoding implementation
│   ├── IWeatherApiService.cs  # Weather API interface
│   ├── WeatherApiService.cs   # Weather API implementation
│   ├── IAutocompleteService.cs # Generic autocomplete interface
│   ├── AutocompleteService.cs  # Autocomplete implementation
│   ├── CitiesDataSource.cs    # Cities data source
│   └── CountriesDataSource.cs # Countries data source (example)
├── Data/                       # Data access layer
│   ├── IWeatherRepository.cs  # Repository interface
│   └── WeatherRepository.cs   # Repository implementation
├── Endpoints/                  # API endpoint definitions
│   ├── Level1Endpoints.cs     # Level 1 API routes
│   ├── Level2Endpoints.cs     # Level 2 API routes
│   └── Level3Endpoints.cs     # Level 3 autocomplete routes
├── Configuration/              # Service configuration
│   └── ServiceConfiguration.cs # DI setup
├── Tests/                      # Comprehensive test suite
│   ├── Services/              # Unit tests
│   └── Integration/           # Integration tests
└── Program.cs                  # Clean entry point (53 lines vs 276!)
```

### Key Architectural Principles
- ✅ **Separation of Concerns**: Each layer has single responsibility
- ✅ **Dependency Injection**: All services properly injected
- ✅ **Repository Pattern**: Clean data access abstraction
- ✅ **Service Layer**: Business logic separated from API concerns
- ✅ **Interface Segregation**: Small, focused interfaces
- ✅ **Plugin Architecture**: Reusable autocomplete with multiple data sources
- ✅ **Comprehensive Testing**: Unit and integration tests

### Level 3 - Reusable Autocomplete Architecture
The new **Level 3** demonstrates advanced architectural patterns:
- 🔌 **Plugin-based Data Sources**: Easy to add new autocomplete data types
- 📂 **Local Data Integration**: Loads 154K+ world cities from local JSON file
- ⚡ **Smart Caching**: In-memory cache with fast loading (2 seconds vs 18+ seconds)
- 🔍 **Intelligent Search**: Prefix matching with relevance ranking
- 📊 **Multiple Data Types**: Cities, countries, easily extensible
- 🛡️ **Error Resilience**: Graceful handling of data source failures
- 🌐 **Offline Support**: No external dependencies for city data

## 🔍 API Endpoints

### Level 1 - External Weather API
- `GET /level1/weatherforecast?city={city}` - Get weather forecast from external API

### Level 2 - Database CRUD Operations
- `GET /level2/weatherforecast/history/{city}` - Get stored weather history
- `POST /level2/weatherforecast/save` - Save weather forecasts to database
- `PUT /level2/weatherforecast/update/{id}` - Update forecast summary
- `DELETE /level2/weatherforecast/delete/{id}` - Delete forecast

### Level 3 - Generic Autocomplete Service
- `GET /level3/cities/autocomplete?query={partialCityName}` - Smart city autocomplete
- `POST /level3/autocomplete` - Generic autocomplete for any data source
- `GET /level3/datasources` - Get available autocomplete data sources
- `GET /level3/datasources/{dataSource}/health` - Check data source health

### Health & Documentation
- `GET /health` - Health check endpoint
- `GET /swagger` - API documentation (development only)
- `GET /` - Application info and available routes

## 🛠️ Development

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL database (for Level 2 features)
- Git

### Building the Application

```bash
# Build solution (includes main project + tests)
dotnet build dotnet-learning-showcase.sln

# Build main project only
dotnet build DotnetLearningShowcase.csproj

# Clean and build
dotnet clean && dotnet build dotnet-learning-showcase.sln

# Release build
dotnet build dotnet-learning-showcase.sln --configuration Release
```

### Running the Application

```bash
# Development mode (hot reload)
dotnet run --project DotnetLearningShowcase.csproj

# Production mode
dotnet run --project DotnetLearningShowcase.csproj --configuration Release

# Specific environment
ASPNETCORE_ENVIRONMENT=Production dotnet run --project DotnetLearningShowcase.csproj

# With custom port
dotnet run --project DotnetLearningShowcase.csproj --urls="http://localhost:8080"
```

The application will be available at:
- HTTP: `http://localhost:5051`
- HTTPS: `https://localhost:7068`

### Environment Variables

```bash
# Database connection (for Level 2)
DATABASE_URL="Host=localhost;Database=weather;Username=user;Password=pass"

# Development environment
ASPNETCORE_ENVIRONMENT=Development

# Custom port for production
ASPNETCORE_URLS=http://+:8080
```

## 🧪 Testing

### Test Architecture
- **Unit Tests**: Service logic with mocked dependencies
- **Integration Tests**: End-to-end API testing
- **Test Frameworks**: xUnit, Moq, FluentAssertions

### Running Tests

```bash
# Run all tests
dotnet test Tests/DotnetLearningShowcase.Tests.csproj

# Run tests with detailed output
dotnet test Tests/DotnetLearningShowcase.Tests.csproj --verbosity normal

# Run solution tests (includes all test projects)
dotnet test dotnet-learning-showcase.sln

# Run tests with coverage
dotnet test Tests/DotnetLearningShowcase.Tests.csproj --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test Tests/DotnetLearningShowcase.Tests.csproj --filter "Integration"
dotnet test Tests/DotnetLearningShowcase.Tests.csproj --filter "Services"

# Run specific test method
dotnet test Tests/DotnetLearningShowcase.Tests.csproj --filter "HealthCheck_ReturnsOk"
```

### Test Coverage

✅ **35/35 tests passing (100%)**

**Service Tests (12 tests):**
- AutocompleteService (7 tests): Core search, data source validation, error handling
- WeatherService (5 tests): Service orchestration, business rules, repository integration

**Integration Tests (23 tests):**
- Level 1 Endpoints (7 tests): External weather API functionality
- Level 2 Endpoints (7 tests): Database CRUD operations  
- Level 3 Endpoints (8 tests): Autocomplete API with data source management
- Integrated Smart Endpoints (6 tests): Cross-level integration and smart features

### Adding New Tests

```csharp
// Unit test example
[Fact]
public async Task YourService_WithValidInput_ReturnsExpectedResult()
{
    // Arrange
    var mockDependency = new Mock<IDependency>();
    var service = new YourService(mockDependency.Object);
    
    // Act
    var result = await service.DoSomethingAsync("input");
    
    // Assert
    result.Should().NotBeNull();
    result.Value.Should().Be("expected");
}

// Integration test example
[Fact]
public async Task YourEndpoint_ReturnsSuccessful()
{
    // Act
    var response = await _client.GetAsync("/your-endpoint");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

## 🚀 Deployment

### Fly.io Deployment (Recommended)

#### Prerequisites
```bash
# Install Fly CLI
curl -L https://fly.io/install.sh | sh

# Login to Fly.io
fly auth login
```

#### Deploy to Fly.io
```bash
# Initial deployment
fly launch --name your-app-name

# Deploy updates
fly deploy

# Set environment variables
fly secrets set DATABASE_URL="your-database-url"

# View logs
fly logs

# Open deployed app
fly open
```

#### Fly.io Configuration
The app includes a `fly.toml` file with:
- Port 8080 configuration
- Health check endpoints
- Auto-scaling settings
- PostgreSQL database connection

### Docker Deployment

```bash
# Build Docker image
docker build -t dotnet-learning-showcase .

# Run locally with Docker
docker run -p 8080:8080 dotnet-learning-showcase

# Push to registry
docker tag dotnet-learning-showcase your-registry/dotnet-learning-showcase
docker push your-registry/dotnet-learning-showcase
```

### Railway Deployment

```bash
# Deploy to Railway (using railway.toml)
railway login
railway deploy
```

### Manual Deployment

```bash
# Publish for production
dotnet publish DotnetLearningShowcase.csproj --configuration Release --output ./publish

# Copy files to server and run
./publish/DotnetLearningShowcase

# Or publish with runtime-specific deployment
dotnet publish DotnetLearningShowcase.csproj --configuration Release --runtime linux-x64 --self-contained --output ./publish
```

## 🗄️ Database Setup

### PostgreSQL Schema
```sql
CREATE TABLE public.weather_forecasts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    city TEXT NOT NULL,
    country TEXT NOT NULL,
    date DATE NOT NULL,
    temperature_c INTEGER NOT NULL,
    summary TEXT
);
```

### Connection Strings
```json
{
  "ConnectionStrings": {
    "SupabaseDb": "Host=localhost;Database=weather;Username=user;Password=password"
  }
}
```

### Database Initialization
The application automatically:
- Creates tables if they don't exist
- Seeds sample data in development
- Handles connection errors gracefully

## 🔧 Configuration

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "ConnectionStrings": {
    "SupabaseDb": "Host=localhost;Database=weather;Username=user;Password=password"
  }
}
```

### Environment-Specific Settings
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production settings
- Environment variables override JSON settings

## 📊 Monitoring & Observability

### Health Checks
```bash
# Check application health
curl http://localhost:5051/health
# Returns: "Healthy"

# Check Level 3 data source health
curl http://localhost:5051/level3/datasources/cities/health
curl http://localhost:5051/level3/datasources/countries/health
```

### Logging
- Structured logging with Serilog
- HTTP request/response logging
- External API call tracking
- Error logging with stack traces

### Metrics
- HTTP request metrics
- Database operation timing
- External API response times
- Error rates

## 🔮 Future Enhancements

The clean architecture makes it easy to add:
- **Caching**: Redis implementation
- **Authentication**: JWT or OAuth2
- **Rate Limiting**: API throttling
- **API Versioning**: Multiple API versions
- **Background Jobs**: Hangfire or Quartz
- **Real-time Updates**: SignalR
- **Monitoring**: Application Insights
- **Documentation**: OpenAPI/Swagger enhancements

## 🤝 Contributing

### Development Workflow
1. Create feature branch: `git checkout -b feature/your-feature`
2. Add models in `Models/`
3. Create service interfaces and implementations in `Services/`
4. Add data access in `Data/` if needed
5. Define endpoints in `Endpoints/`
6. Write comprehensive tests
7. Update documentation
8. Submit pull request

### Code Quality
- Follow SOLID principles
- Write unit tests for all business logic
- Add integration tests for new endpoints
- Use dependency injection for all services
- Implement proper error handling
- Add XML documentation for public APIs

## 📋 Migration Notes

This refactored application is **100% backward compatible**:
- ✅ All existing endpoints work identically
- ✅ Same request/response formats
- ✅ Same database schema
- ✅ Same deployment configurations
- ✅ Same environment variables

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Built with .NET 8, Clean Architecture, and ❤️** 