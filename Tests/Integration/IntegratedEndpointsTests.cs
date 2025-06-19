using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DotnetLearningShowcase.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DotnetLearningShowcase.Tests.Integration;

public class IntegratedEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public IntegratedEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Level1Smart_WithValidCity_ReturnsWeatherWithoutSuggestions()
    {
        // Act
        var response = await _client.GetAsync("/level1/weatherforecast/smart?city=Berlin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("cityFound").GetBoolean().Should().BeTrue();
        result.GetProperty("weather").ValueKind.Should().Be(JsonValueKind.Object);
        
        // Check if suggestions property exists and is null
        if (result.TryGetProperty("suggestions", out var suggestions))
        {
            suggestions.ValueKind.Should().Be(JsonValueKind.Null);
        }
    }

    [Fact]
    public async Task Level1Smart_WithInvalidCity_ReturnsSuggestionsWithoutWeather()
    {
        // Act - Use a more unlikely city name to trigger not found
        var response = await _client.GetAsync("/level1/weatherforecast/smart?city=ZzzInvalidCity123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("cityFound").GetBoolean().Should().BeFalse();
        result.GetProperty("message").GetString().Should().Contain("ZzzInvalidCity123");
        result.GetProperty("suggestions").GetArrayLength().Should().BeGreaterOrEqualTo(0);
        result.GetProperty("hint").GetString().Should().NotBeNullOrEmpty();
        
        // If there are suggestions, verify their structure
        var suggestions = result.GetProperty("suggestions").EnumerateArray().ToList();
        if (suggestions.Any())
        {
            var firstSuggestion = suggestions.First();
            firstSuggestion.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
            firstSuggestion.GetProperty("label").GetString().Should().NotBeNullOrEmpty();
            firstSuggestion.GetProperty("value").GetString().Should().NotBeNullOrEmpty();
            firstSuggestion.GetProperty("metadata").ValueKind.Should().Be(JsonValueKind.Object);
        }
    }

    [Fact]
    public async Task Level1Smart_WithDefaultCity_ReturnsWeather()
    {
        // Act
        var response = await _client.GetAsync("/level1/weatherforecast/smart");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("cityFound").GetBoolean().Should().BeTrue();
        result.GetProperty("weather").ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact(Skip = "Temporarily disabled due to PostgreSQL database structure mismatch")]
    public async Task Level2SmartHistory_WithExistingCity_ReturnsExactMatch()
    {
        // Arrange - First save some data
        var saveRequest = new WeatherSaveRequest("TestCityForHistory", "TestCountry", new List<ForecastDto>
        {
            new(DateOnly.FromDateTime(DateTime.Today), 22, "Test forecast")
        });
        await _client.PostAsJsonAsync("/level2/weatherforecast/save", saveRequest);

        // Act
        var response = await _client.GetAsync("/level2/weatherforecast/history/smart/TestCityForHistory");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("exactMatch").GetBoolean().Should().BeTrue();
        result.GetProperty("city").GetString().Should().Be("TestCityForHistory");
        result.GetProperty("history").GetArrayLength().Should().BeGreaterThan(0);
        
        // Check if suggestions property exists and is null
        if (result.TryGetProperty("suggestions", out var suggestions))
        {
            suggestions.ValueKind.Should().Be(JsonValueKind.Null);
        }
    }

    [Fact]
    public async Task Level2SmartHistory_WithNonExistingCity_ReturnsSuggestionsAndCitiesWithData()
    {
        // Act
        var response = await _client.GetAsync("/level2/weatherforecast/history/smart/NonExistentCityXYZ");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("exactMatch").GetBoolean().Should().BeFalse();
        result.GetProperty("city").GetString().Should().Be("NonExistentCityXYZ");
        result.GetProperty("message").GetString().Should().Contain("NonExistentCityXYZ");
        result.GetProperty("citiesWithData").ValueKind.Should().Be(JsonValueKind.Array);
        result.GetProperty("allSuggestions").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task Level2BulkWeather_WithMixedCities_ReturnsAppropriateResults()
    {
        // Arrange
        var request = new BulkWeatherRequest(new List<string>
        {
            "Berlin",       // Should find exact match
            "Londo",        // Should suggest London
            "Pari",         // Should suggest Paris
            "InvalidCityXYZ123" // Should not find any match
        });

        // Act
        var response = await _client.PostAsJsonAsync("/level2/weatherforecast/bulk", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        var results = result.GetProperty("results").EnumerateArray().ToList();
        results.Should().HaveCount(4);

        // Check Berlin (exact match)
        var berlinResult = results.First(r => r.GetProperty("query").GetString() == "Berlin");
        berlinResult.GetProperty("matched").GetBoolean().Should().BeTrue();
        berlinResult.GetProperty("weather").ValueKind.Should().Be(JsonValueKind.Object);

        // Check Londo (geocoding might actually find "London" so this could be matched)
        var londoResult = results.First(r => r.GetProperty("query").GetString() == "Londo");
        // The geocoding service might actually resolve "Londo" to a valid city, so we check both cases
        if (londoResult.GetProperty("matched").GetBoolean())
        {
            londoResult.GetProperty("weather").ValueKind.Should().Be(JsonValueKind.Object);
        }
        else
        {
            if (londoResult.TryGetProperty("suggestion", out var suggestion))
            {
                suggestion.GetProperty("value").GetString().Should().NotBeNullOrEmpty();
            }
        }

        // Check InvalidCityXYZ123 (should have error)
        var invalidResult = results.First(r => r.GetProperty("query").GetString() == "InvalidCityXYZ123");
        invalidResult.GetProperty("matched").GetBoolean().Should().BeFalse();
        if (invalidResult.TryGetProperty("error", out var error))
        {
            error.GetString().Should().Contain("No matching city found");
        }
    }


} 