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

## Technologies Used

- .NET 8
- ASP.NET Core
- xUnit
- FluentAssertions
- Open-Meteo API

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.