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