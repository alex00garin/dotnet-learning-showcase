using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Level2.WeatherCrud.Tests;

public class WeatherForecastTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WeatherForecastTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:SupabaseDb"] = "Host=aws-0-eu-west-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.udwbtmwlmgxedfevddre;Password=O6On1ka3wfViZ7Sb;SSL Mode=Require;Trust Server Certificate=true"
                });
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", content);
    }

    [Fact]
    public async Task SaveWeatherForecast_ReturnsOk()
    {
        // Arrange
        var request = new
        {
            city = "TestCity",
            country = "TestCountry",
            forecasts = new[]
            {
                new
                {
                    date = "2024-03-20",
                    temperatureC = 20,
                    summary = "Sunny"
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/weatherforecast/save", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Saved", content);
    }

    [Fact]
    public async Task GetWeatherHistory_ReturnsOk()
    {
        // Arrange
        var city = "TestCity";

        // Act
        var response = await _client.GetAsync($"/weatherforecast/history/{city}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<List<WeatherForecastRecord>>();
        Assert.NotNull(content);
    }

    [Fact]
    public async Task UpdateWeatherForecast_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new { summary = "Updated Summary" };

        // Act
        var response = await _client.PutAsJsonAsync($"/weatherforecast/update/{id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteWeatherForecast_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/weatherforecast/delete/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

// DTOs for testing
public record WeatherForecastRecord(
    Guid id,
    string city,
    string country,
    DateTime date,
    int temperaturec,
    string summary
); 