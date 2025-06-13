using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DotnetLearningShowcase.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotnetLearningShowcase.Tests.Integration;

public class WeatherEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WeatherEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("\"Healthy\"");
    }

    [Fact]
    public async Task Level1_WeatherForecast_WithValidCity_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/level1/weatherforecast?city=Berlin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var weatherResponse = JsonSerializer.Deserialize<WeatherApiResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        weatherResponse.Should().NotBeNull();
        weatherResponse!.Location.Should().Contain("Berlin");
        weatherResponse.Forecasts.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Level1_WeatherForecast_WithInvalidCity_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/level1/weatherforecast?city=NonexistentCity12345");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Level1_WeatherForecast_WithoutCity_UsesDefaultCity()
    {
        // Act
        var response = await _client.GetAsync("/level1/weatherforecast");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var weatherResponse = JsonSerializer.Deserialize<WeatherApiResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        weatherResponse.Should().NotBeNull();
        weatherResponse!.Location.Should().Contain("Berlin"); // Default city
    }

    [Fact]
    public async Task Level1_HourlyWeatherForecast_WithValidCity_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/level1/weatherforecast/hourly?city=Berlin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var hourlyResponse = JsonSerializer.Deserialize<HourlyWeatherComparisonResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        hourlyResponse.Should().NotBeNull();
        hourlyResponse!.Location.Should().Contain("Berlin");
        hourlyResponse.TodayForecasts.Should().NotBeEmpty();
        hourlyResponse.TodayForecasts.Should().HaveCount(24); // Should have 24 hours for today
        hourlyResponse.YesterdayForecasts.Should().NotBeEmpty();
        hourlyResponse.YesterdayForecasts.Should().HaveCount(24); // Should have 24 hours for yesterday
        hourlyResponse.Comparison.Should().NotBeNull();
        hourlyResponse.Comparison.ComparisonText.Should().NotBeNullOrEmpty();
        
        // Verify each forecast has required properties
        var firstTodayForecast = hourlyResponse.TodayForecasts.First();
        firstTodayForecast.TemperatureC.Should().BeGreaterThan(-50).And.BeLessThan(60);
        firstTodayForecast.WeatherDescription.Should().NotBeNullOrEmpty();
        firstTodayForecast.TemperatureF.Should().BeGreaterThan(-50).And.BeLessThan(140);
        
        var firstYesterdayForecast = hourlyResponse.YesterdayForecasts.First();
        firstYesterdayForecast.TemperatureC.Should().BeGreaterThan(-50).And.BeLessThan(60);
        firstYesterdayForecast.WeatherDescription.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Level1_HourlyWeatherForecast_WithInvalidCity_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/level1/weatherforecast/hourly?city=NonexistentCity12345");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Level1_HourlyWeatherForecast_WithoutCity_UsesDefaultCity()
    {
        // Act
        var response = await _client.GetAsync("/level1/weatherforecast/hourly");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var hourlyResponse = JsonSerializer.Deserialize<HourlyWeatherComparisonResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        hourlyResponse.Should().NotBeNull();
        hourlyResponse!.Location.Should().Contain("Berlin"); // Default city
        hourlyResponse.TodayForecasts.Should().HaveCount(24);
        hourlyResponse.YesterdayForecasts.Should().HaveCount(24);
        hourlyResponse.Comparison.Should().NotBeNull();
    }

    [Fact]
    public async Task Level1_SmartHourlyWeatherForecast_WithValidCity_ReturnsOkWithWeatherData()
    {
        // Act
        var response = await _client.GetAsync("/level1/weatherforecast/hourly/smart?city=Berlin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        root.TryGetProperty("weather", out var weatherProp).Should().BeTrue();
        root.TryGetProperty("cityFound", out var cityFoundProp).Should().BeTrue();
        cityFoundProp.GetBoolean().Should().BeTrue();

        weatherProp.TryGetProperty("location", out var locationProp).Should().BeTrue();
        locationProp.GetString().Should().Contain("Berlin");
    }

    [Fact]
    public async Task Level1_SmartHourlyWeatherForecast_WithInvalidCity_ReturnsNotFoundWithSuggestions()
    {
        // Act
        var response = await _client.GetAsync("/level1/weatherforecast/hourly/smart?city=NotACity");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        root.TryGetProperty("message", out var messageProp).Should().BeTrue();
        messageProp.GetString().Should().Contain("NotACity");
        
        root.TryGetProperty("cityFound", out var cityFoundProp).Should().BeTrue();
        cityFoundProp.GetBoolean().Should().BeFalse();
        
        root.TryGetProperty("hint", out var hintProp).Should().BeTrue();
        hintProp.GetString().Should().Contain("autocomplete");
    }

    [Fact]
    public async Task Level1_SmartHourlyWeatherForecast_WithPartialCityName_FindsCityAndReturnsWeather()
    {
        // Act
        var response = await _client.GetAsync("/level1/weatherforecast/hourly/smart?city=Lond");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        root.TryGetProperty("weather", out var weatherProp).Should().BeTrue();
        root.TryGetProperty("cityFound", out var cityFoundProp).Should().BeTrue();
        cityFoundProp.GetBoolean().Should().BeTrue();

        weatherProp.TryGetProperty("location", out var locationProp).Should().BeTrue();
        locationProp.GetString().Should().Contain("London");
    }

    [Theory]
    [InlineData("Tokyo")]
    [InlineData("Paris")]
    [InlineData("Sydney")]
    [InlineData("Cairo")]
    public async Task Level1_HourlyWeatherForecast_WithDifferentCities_ReturnsValidData(string city)
    {
        // Act
        var response = await _client.GetAsync($"/level1/weatherforecast/hourly?city={city}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var hourlyResponse = JsonSerializer.Deserialize<HourlyWeatherComparisonResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        hourlyResponse.Should().NotBeNull();
        hourlyResponse!.Location.Should().Contain(city);
        hourlyResponse.TodayForecasts.Should().HaveCount(24);
        hourlyResponse.YesterdayForecasts.Should().HaveCount(24);
        hourlyResponse.Comparison.Should().NotBeNull();
        
        // Verify data quality for today's forecasts
        foreach (var forecast in hourlyResponse.TodayForecasts)
        {
            forecast.TemperatureC.Should().BeGreaterThan(-60).And.BeLessThan(60);
            forecast.Precipitation.Should().BeGreaterOrEqualTo(0);
            forecast.WeatherCode.Should().BeGreaterOrEqualTo(0);
            forecast.WeatherDescription.Should().NotBeNullOrEmpty();
        }
        
        // Verify data quality for yesterday's forecasts
        foreach (var forecast in hourlyResponse.YesterdayForecasts)
        {
            forecast.TemperatureC.Should().BeGreaterThan(-60).And.BeLessThan(60);
            forecast.Precipitation.Should().BeGreaterOrEqualTo(0);
            forecast.WeatherCode.Should().BeGreaterOrEqualTo(0);
            forecast.WeatherDescription.Should().NotBeNullOrEmpty();
        }
        
        // Verify comparison data
        hourlyResponse.Comparison.TodayAverageTemp.Should().BeGreaterThan(-60).And.BeLessThan(60);
        hourlyResponse.Comparison.YesterdayAverageTemp.Should().BeGreaterThan(-60).And.BeLessThan(60);
        hourlyResponse.Comparison.ComparisonText.Should().NotBeNullOrEmpty();
        hourlyResponse.Comparison.ComparisonText.Should().MatchRegex(@"Today.*(warmer|colder|similar).*yesterday");
    }

    [Fact]
    public async Task Level2_SaveWeatherForecast_ReturnsSuccess()
    {
        // Arrange
        var request = new WeatherSaveRequest("TestCity", "TestCountry", new List<ForecastDto>
        {
            new(DateOnly.FromDateTime(DateTime.Today), 25, "Sunny test weather")
        });

        // Act
        var response = await _client.PostAsJsonAsync("/level2/weatherforecast/save", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Saved");
    }

    [Theory]
    [InlineData("/level1")]
    [InlineData("/level2")]
    public async Task LevelEndpoints_ReturnDescription(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RootEndpoint_ReturnsWelcomeMessage()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Dotnet Learning Showcase");
    }
} 