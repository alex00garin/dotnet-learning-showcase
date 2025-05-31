using System.Net;
using System.Text;
using System.Text.Json;
using AdvancedWebApi.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AdvancedWebApi.Tests;

public class WeatherControllerTests : IClassFixture<WebApplicationFactory<AdvancedWebApi.Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public WeatherControllerTests(WebApplicationFactory<AdvancedWebApi.Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task GetWeatherHistory_ReturnsOk_ForValidLocation()
    {
        // Arrange
        var location = "London";

        // Act
        var response = await _client.GetAsync($"/api/weather/{location}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var weatherRecords = JsonSerializer.Deserialize<List<WeatherRecord>>(content, _jsonOptions);
        weatherRecords.Should().NotBeNull();
        weatherRecords.Should().AllSatisfy(w => w.Location.Should().Be(location));
    }

    [Fact]
    public async Task GetCurrentWeather_ReturnsOk_ForValidLocation()
    {
        // Arrange
        var location = "London";

        // Act
        var response = await _client.GetAsync($"/api/weather/{location}/current");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var weatherRecord = JsonSerializer.Deserialize<WeatherRecord>(content, _jsonOptions);
        weatherRecord.Should().NotBeNull();
        weatherRecord!.Location.Should().Be(location);
    }

    [Fact]
    public async Task AddWeatherRecord_ReturnsCreated_ForValidData()
    {
        // Arrange
        var newRecord = new WeatherRecord
        {
            Location = "Paris",
            Temperature = 22.5,
            Humidity = 70,
            WindSpeed = 10.0,
            WindDirection = "NE",
            Pressure = 1013.2,
            Conditions = "Sunny"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(newRecord),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/weather", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdRecord = JsonSerializer.Deserialize<WeatherRecord>(responseContent, _jsonOptions);
        createdRecord.Should().NotBeNull();
        createdRecord!.Location.Should().Be(newRecord.Location);
        createdRecord.Temperature.Should().Be(newRecord.Temperature);
    }

    [Fact]
    public async Task AddWeatherRecord_ReturnsBadRequest_ForInvalidData()
    {
        // Arrange
        var invalidRecord = new WeatherRecord
        {
            Location = "", // Invalid empty location
            Temperature = 22.5
        };

        var content = new StringContent(
            JsonSerializer.Serialize(invalidRecord),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/weather", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetWeatherHistory_ReturnsNotFound_ForInvalidLocation()
    {
        // Arrange
        var invalidLocation = "NonExistentCity123";

        // Act
        var response = await _client.GetAsync($"/api/weather/{invalidLocation}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
} 