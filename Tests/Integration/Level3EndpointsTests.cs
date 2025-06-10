using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DotnetLearningShowcase.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DotnetLearningShowcase.Tests.Integration;

public class Level3EndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public Level3EndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Level3_Root_ReturnsDescription()
    {
        // Act
        var response = await _client.GetAsync("/level3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Level 3 - Generic Autocomplete Service");
    }

    [Fact]
    public async Task GetDataSources_ReturnsAvailableDataSources()
    {
        // Act
        var response = await _client.GetAsync("/level3/datasources");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("dataSources").GetArrayLength().Should().BeGreaterThan(0);
        
        var dataSources = result.GetProperty("dataSources").EnumerateArray()
            .Select(x => x.GetString()).ToList();
        
        dataSources.Should().Contain("cities");
        dataSources.Should().Contain("countries");
    }

    [Fact]
    public async Task CityAutocomplete_WithValidQuery_ReturnsResults()
    {
        // Act
        var response = await _client.GetAsync("/level3/cities/autocomplete?query=Lon&limit=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AutocompleteResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Query.Should().Be("Lon");
        result.DataSource.Should().Be("cities");
        result.Results.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThan(0);
        
        // Verify result structure
        var firstResult = result.Results.First();
        firstResult.Id.Should().NotBeNullOrEmpty();
        firstResult.Label.Should().NotBeNullOrEmpty();
        firstResult.Value.Should().NotBeNullOrEmpty();
        firstResult.Metadata.Should().NotBeNull();
    }

    [Fact]
    public async Task CityAutocomplete_WithEmptyQuery_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/level3/cities/autocomplete?query=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CityAutocomplete_WithoutQuery_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/level3/cities/autocomplete");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenericAutocomplete_WithCitiesDataSource_ReturnsResults()
    {
        // Arrange
        var request = new AutocompleteRequest("Berlin", 3, "cities");

        // Act
        var response = await _client.PostAsJsonAsync("/level3/autocomplete", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AutocompleteResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Query.Should().Be("Berlin");
        result.DataSource.Should().Be("cities");
        result.Results.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenericAutocomplete_WithInvalidDataSource_ReturnsEmptyResults()
    {
        // Arrange
        var request = new AutocompleteRequest("test", 5, "invalid");

        // Act
        var response = await _client.PostAsJsonAsync("/level3/autocomplete", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AutocompleteResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.DataSource.Should().Be("invalid");
        result.Results.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
} 