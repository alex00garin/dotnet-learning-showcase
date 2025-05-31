using System.Net;
using System.Text;
using System.Text.Json;
using AdvancedWebApi.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AdvancedWebApi.Tests;

public class ForecastLogsControllerTests : IClassFixture<WebApplicationFactory<AdvancedWebApi.Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ForecastLogsControllerTests(WebApplicationFactory<AdvancedWebApi.Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task GetAllForecastLogs_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/forecastlogs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var logs = JsonSerializer.Deserialize<List<ForecastLog>>(content, _jsonOptions);
        logs.Should().NotBeNull();
    }

    [Fact]
    public async Task GetForecastLogById_ReturnsOk_ForValidId()
    {
        // Arrange
        var id = 1; // Assuming this ID exists in the seeded data

        // Act
        var response = await _client.GetAsync($"/api/forecastlogs/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var log = JsonSerializer.Deserialize<ForecastLog>(content, _jsonOptions);
        log.Should().NotBeNull();
        log!.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetForecastLogById_ReturnsNotFound_ForInvalidId()
    {
        // Arrange
        var invalidId = 999; // Assuming this ID doesn't exist

        // Act
        var response = await _client.GetAsync($"/api/forecastlogs/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetForecastLogsByCity_ReturnsOk_ForValidCity()
    {
        // Arrange
        var city = "London";

        // Act
        var response = await _client.GetAsync($"/api/forecastlogs/city/{city}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var logs = JsonSerializer.Deserialize<List<ForecastLog>>(content, _jsonOptions);
        logs.Should().NotBeNull();
        logs.Should().AllSatisfy(l => l.City.Should().Be(city));
    }

    [Fact]
    public async Task CreateForecastLog_ReturnsCreated_ForValidData()
    {
        // Arrange
        var newLog = new ForecastLog
        {
            City = "Berlin",
            Summary = "Partly cloudy with light rain",
            TemperatureC = 18.5f
        };

        var content = new StringContent(
            JsonSerializer.Serialize(newLog),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/forecastlogs", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdLog = JsonSerializer.Deserialize<ForecastLog>(responseContent, _jsonOptions);
        createdLog.Should().NotBeNull();
        createdLog!.City.Should().Be(newLog.City);
        createdLog.Summary.Should().Be(newLog.Summary);
        createdLog.TemperatureC.Should().Be(newLog.TemperatureC);
    }

    [Fact]
    public async Task UpdateForecastLog_ReturnsNoContent_ForValidData()
    {
        // Arrange
        var id = 1; // Assuming this ID exists in the seeded data
        var updatedLog = new ForecastLog
        {
            Id = id,
            City = "London",
            Summary = "Updated forecast",
            TemperatureC = 20.0f
        };

        var content = new StringContent(
            JsonSerializer.Serialize(updatedLog),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PutAsync($"/api/forecastlogs/{id}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the log is actually updated
        var getResponse = await _client.GetAsync($"/api/forecastlogs/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ForecastLog>(getContent, _jsonOptions);
        result.Should().NotBeNull();
        result!.Summary.Should().Be(updatedLog.Summary);
        result.TemperatureC.Should().Be(updatedLog.TemperatureC);
    }

    [Fact]
    public async Task DeleteForecastLog_ReturnsNoContent_ForValidId()
    {
        // Arrange: create a fresh forecast log to delete
        var newLog = new ForecastLog
        {
            City = "TestCity",
            Summary = "Test delete log",
            TemperatureC = 25.0f
        };
        var createContent = new StringContent(
            JsonSerializer.Serialize(newLog),
            Encoding.UTF8,
            "application/json");
        var createResponse = await _client.PostAsync("/api/forecastlogs", createContent);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdLog = JsonSerializer.Deserialize<ForecastLog>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
        createdLog.Should().NotBeNull();
        var id = createdLog!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/forecastlogs/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the log is actually deleted
        var getResponse = await _client.GetAsync($"/api/forecastlogs/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteForecastLog_ReturnsNotFound_ForInvalidId()
    {
        // Arrange
        var invalidId = 999; // Assuming this ID doesn't exist

        // Act
        var response = await _client.DeleteAsync($"/api/forecastlogs/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
} 