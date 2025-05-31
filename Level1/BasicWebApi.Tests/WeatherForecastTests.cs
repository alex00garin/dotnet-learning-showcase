using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BasicWebApi.Tests;

public class WeatherForecastTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WeatherForecastTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ReturnsWeatherForecast_ForValidCity()
    {
        var response = await _client.GetAsync("/weatherforecast?city=Cardiff");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK, "valid city should return OK status");
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json", "response should be JSON");

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        
        var location = doc.RootElement.GetProperty("Location").GetString();
        location.Should().Contain("Cardiff", "response should contain the requested city name");

        var forecasts = doc.RootElement.GetProperty("Forecasts");
        forecasts.GetArrayLength().Should().BeGreaterThan(0, "should return at least one forecast");
    }

    [Fact]
    public async Task Returns404_ForInvalidCity()
    {
        var response = await _client.GetAsync("/weatherforecast?city=UnknownCityXYZ123");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "non-existent city should return 404");
    }

    [Fact]
    public async Task Returns400_ForMissingCityParameter()
    {
        var response = await _client.GetAsync("/weatherforecast");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "missing city parameter should return 400");
    }
}
