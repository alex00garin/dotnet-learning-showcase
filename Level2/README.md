# 🌦️ Level 2: Advanced Weather API with .NET 9

This project demonstrates a more advanced weather API implementation using .NET 9, featuring a PostgreSQL database, Entity Framework Core, and comprehensive API documentation with Swagger.

## 🚀 Features

- **Weather Records Management**
  - Store and retrieve historical weather data
  - Track current weather conditions
  - Support for multiple locations

- **Forecast Logging System**
  - Log weather forecast requests
  - Track forecast accuracy
  - Historical forecast analysis

- **Modern Tech Stack**
  - .NET 9 Web API
  - PostgreSQL database
  - Entity Framework Core
  - Swagger/OpenAPI documentation
  - RESTful API design

## 📋 Prerequisites

- .NET 9 SDK
- PostgreSQL 14 or later
- Your favorite IDE (VS Code, Visual Studio, Rider)

## 🛠️ Setup

1. **Install PostgreSQL**
   ```bash
   # macOS
   brew install postgresql@14
   brew services start postgresql@14
   ```

2. **Create Database User**
   ```bash
   createuser -s postgres
   ```

3. **Create Database**
   ```bash
   createdb weatherdb
   ```

4. **Configure Connection**
   - Update `appsettings.json` with your database credentials if needed
   - Default connection string:
     ```
     Host=localhost;Port=5432;Database=weatherdb;Username=postgres;Password=postgres
     ```

5. **Apply Database Migrations**
   ```bash
   # Navigate to the project directory
   cd Level2/AdvancedWebApi
   dotnet ef database update
   ```

## 🏃‍♂️ Running the Application

1. **Start the API**
   ```bash
   # Navigate to the project directory
   cd Level2/AdvancedWebApi
   dotnet run
   ```

2. **Access Swagger UI**
   - Open `http://localhost:5260/swagger` in your browser
   - Explore and test the API endpoints

## 📚 API Endpoints

### Weather Records
- `GET /api/weather/{location}` - Get weather history
- `GET /api/weather/{location}/current` - Get current weather
- `POST /api/weather` - Add new weather record

### Forecast Logs
- `GET /api/forecastlogs` - List all forecast logs
- `GET /api/forecastlogs/{id}` - Get specific forecast log
- `GET /api/forecastlogs/city/{city}` - Get logs for a city
- `POST /api/forecastlogs` - Create new forecast log
- `PUT /api/forecastlogs/{id}` - Update forecast log
- `DELETE /api/forecastlogs/{id}` - Delete forecast log

## 🧪 Testing

The project includes a test suite in the `AdvancedWebApi.Tests` directory. Run tests using:

```bash
# Navigate to the test project directory
cd Level2/AdvancedWebApi.Tests
dotnet test
```

## 📦 Project Structure

```
Level2/
├── AdvancedWebApi/              # Main API project
│   ├── Controllers/            # API endpoints
│   ├── Data/                   # Database context
│   ├── Models/                 # Data models
│   └── Migrations/             # Database migrations
└── AdvancedWebApi.Tests/       # Test project
```

## 🔄 Database Schema

### WeatherRecord
- `Id` (int, PK)
- `Location` (string)
- `Timestamp` (datetime)
- `Temperature` (double)
- `Humidity` (double)
- `WindSpeed` (double)
- `WindDirection` (string)
- `Pressure` (double)
- `Conditions` (string)

### ForecastLog
- `Id` (int, PK)
- `City` (string)
- `RequestedAt` (datetime)
- `Summary` (string)
- `TemperatureC` (float)
- `TemperatureF` (float, computed)

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details. 