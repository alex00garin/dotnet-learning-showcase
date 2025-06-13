using DotnetLearningShowcase.Data;
using DotnetLearningShowcase.Models;
using DotnetLearningShowcase.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotnetLearningShowcase.Tests.Services;

public class WeatherServiceTests
{
    private readonly Mock<IGeocodingService> _geocodingServiceMock;
    private readonly Mock<IWeatherApiService> _weatherApiServiceMock;
    private readonly Mock<IWeatherRepository> _weatherRepositoryMock;
    private readonly WeatherService _weatherService;

    public WeatherServiceTests()
    {
        _geocodingServiceMock = new Mock<IGeocodingService>();
        _weatherApiServiceMock = new Mock<IWeatherApiService>();
        _weatherRepositoryMock = new Mock<IWeatherRepository>();
        _weatherService = new WeatherService(
            _geocodingServiceMock.Object,
            _weatherApiServiceMock.Object,
            _weatherRepositoryMock.Object);
    }

    [Fact]
    public async Task GetWeatherForecastAsync_WithValidCity_ReturnsWeatherResponse()
    {
        // Arrange
        var city = "Berlin";
        var location = new LocationData(52.5200, 13.4050, "Berlin", "Germany");
        var forecasts = new[]
        {
            new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), 20, "Sunny in Berlin, Germany"),
            new WeatherForecast(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), 18, "Cloudy in Berlin, Germany")
        };

        _geocodingServiceMock
            .Setup(x => x.GetLocationAsync(city))
            .ReturnsAsync(location);

        _weatherApiServiceMock
            .Setup(x => x.GetForecastsAsync(location))
            .ReturnsAsync(forecasts);

        // Act
        var result = await _weatherService.GetWeatherForecastAsync(city);

        // Assert
        result.Should().NotBeNull();
        result!.Location.Should().Be("Berlin, Germany");
        result.Forecasts.Should().HaveCount(2);
        result.Forecasts.First().Summary.Should().Be("Sunny in Berlin, Germany");
    }

    [Fact]
    public async Task GetWeatherForecastAsync_WithInvalidCity_ReturnsNull()
    {
        // Arrange
        var city = "NonexistentCity";

        _geocodingServiceMock
            .Setup(x => x.GetLocationAsync(city))
            .ReturnsAsync((LocationData?)null);

        // Act
        var result = await _weatherService.GetWeatherForecastAsync(city);

        // Assert
        result.Should().BeNull();
        _weatherApiServiceMock.Verify(x => x.GetForecastsAsync(It.IsAny<LocationData>()), Times.Never);
    }

    [Fact]
    public async Task GetHourlyWeatherForecastAsync_WithValidCity_ReturnsHourlyWeatherResponse()
    {
        // Arrange
        var city = "Berlin";
        var location = new LocationData(52.5200, 13.4050, "Berlin", "Germany");
        var hourlyForecasts = new[]
        {
            new HourlyWeatherForecast(
                DateTime.Now,
                22.5,
                0.0,
                0,
                15.2,
                65.0,
                20.8
            ),
            new HourlyWeatherForecast(
                DateTime.Now.AddHours(1),
                21.8,
                0.2,
                1,
                14.5,
                68.0,
                19.9
            )
        };

        _geocodingServiceMock
            .Setup(x => x.GetLocationAsync(city))
            .ReturnsAsync(location);

        _weatherApiServiceMock
            .Setup(x => x.GetHourlyForecastsAsync(location))
            .ReturnsAsync(hourlyForecasts);

        // Act
        var result = await _weatherService.GetHourlyWeatherForecastAsync(city);

        // Assert
        result.Should().NotBeNull();
        result!.Location.Should().Be("Berlin, Germany");
        result.HourlyForecasts.Should().HaveCount(2);
        result.HourlyForecasts.First().TemperatureC.Should().Be(22.5);
        result.HourlyForecasts.First().WeatherDescription.Should().Be("Clear sky");
        result.HourlyForecasts.First().TemperatureF.Should().BeApproximately(72.5, 0.1);
    }

    [Fact]
    public async Task GetHourlyWeatherForecastAsync_WithInvalidCity_ReturnsNull()
    {
        // Arrange
        var city = "NonexistentCity";

        _geocodingServiceMock
            .Setup(x => x.GetLocationAsync(city))
            .ReturnsAsync((LocationData?)null);

        // Act
        var result = await _weatherService.GetHourlyWeatherForecastAsync(city);

        // Assert
        result.Should().BeNull();
        _weatherApiServiceMock.Verify(x => x.GetHourlyForecastsAsync(It.IsAny<LocationData>()), Times.Never);
    }

    [Fact]
    public async Task GetHourlyWeatherForecastAsync_WithDefaultCity_UsesDefaultBerlin()
    {
        // Arrange
        var location = new LocationData(52.5200, 13.4050, "Berlin", "Germany");
        var hourlyForecasts = new[]
        {
            new HourlyWeatherForecast(DateTime.Now, 20.0, 0.0, 1, 10.0, 60.0, 18.5)
        };

        _geocodingServiceMock
            .Setup(x => x.GetLocationAsync("Berlin"))
            .ReturnsAsync(location);

        _weatherApiServiceMock
            .Setup(x => x.GetHourlyForecastsAsync(location))
            .ReturnsAsync(hourlyForecasts);

        // Act
        var result = await _weatherService.GetHourlyWeatherForecastAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Location.Should().Be("Berlin, Germany");
        _geocodingServiceMock.Verify(x => x.GetLocationAsync("Berlin"), Times.Once);
    }

    [Fact]
    public async Task SaveForecastAsync_CallsRepository()
    {
        // Arrange
        var request = new WeatherSaveRequest("London", "UK", new List<ForecastDto>
        {
            new(DateOnly.FromDateTime(DateTime.Today), 15, "Rainy")
        });

        // Act
        await _weatherService.SaveForecastAsync(request);

        // Assert
        _weatherRepositoryMock.Verify(
            x => x.SaveForecastsAsync("London", "UK", request.Forecasts),
            Times.Once);
    }

    [Fact]
    public async Task UpdateSummaryAsync_CallsRepository()
    {
        // Arrange
        var id = Guid.NewGuid();
        var summary = "Updated summary";

        _weatherRepositoryMock
            .Setup(x => x.UpdateSummaryAsync(id, summary))
            .ReturnsAsync(true);

        // Act
        var result = await _weatherService.UpdateSummaryAsync(id, summary);

        // Assert
        result.Should().BeTrue();
        _weatherRepositoryMock.Verify(x => x.UpdateSummaryAsync(id, summary), Times.Once);
    }

    [Fact]
    public async Task DeleteForecastAsync_CallsRepository()
    {
        // Arrange
        var id = Guid.NewGuid();

        _weatherRepositoryMock
            .Setup(x => x.DeleteForecastAsync(id))
            .ReturnsAsync(true);

        // Act
        var result = await _weatherService.DeleteForecastAsync(id);

        // Assert
        result.Should().BeTrue();
        _weatherRepositoryMock.Verify(x => x.DeleteForecastAsync(id), Times.Once);
    }
} 