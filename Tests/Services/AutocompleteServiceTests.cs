using DotnetLearningShowcase.Models;
using DotnetLearningShowcase.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotnetLearningShowcase.Tests.Services;

public class AutocompleteServiceTests
{
    private readonly Mock<IAutocompleteDataSource<object>> _citiesDataSourceMock;
    private readonly Mock<IAutocompleteDataSource<object>> _countriesDataSourceMock;
    private readonly Mock<ILogger<AutocompleteService>> _loggerMock;
    private readonly AutocompleteService _autocompleteService;

    public AutocompleteServiceTests()
    {
        _citiesDataSourceMock = new Mock<IAutocompleteDataSource<object>>();
        _countriesDataSourceMock = new Mock<IAutocompleteDataSource<object>>();
        _loggerMock = new Mock<ILogger<AutocompleteService>>();

        // Setup data source names
        _citiesDataSourceMock.Setup(x => x.Name).Returns("cities");
        _countriesDataSourceMock.Setup(x => x.Name).Returns("countries");

        var dataSources = new[]
        {
            _citiesDataSourceMock.Object,
            _countriesDataSourceMock.Object
        };

        _autocompleteService = new AutocompleteService(dataSources, _loggerMock.Object);
    }

    [Fact]
    public async Task SearchAsync_WithValidDataSource_ReturnsResults()
    {
        // Arrange
        var request = new AutocompleteRequest("Lon", 5, "cities");
        var expectedResults = new List<AutocompleteItem>
        {
            new("london_gb", "London, GB", "London", new Dictionary<string, object> { ["country"] = "GB" }),
            new("london_ca", "London, CA", "London", new Dictionary<string, object> { ["country"] = "CA" })
        };

        _citiesDataSourceMock
            .Setup(x => x.SearchAsync("Lon", 5))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _autocompleteService.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Query.Should().Be("Lon");
        result.DataSource.Should().Be("cities");
        result.Results.Should().HaveCount(2);
        result.Results.Should().BeEquivalentTo(expectedResults);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task SearchAsync_WithInvalidDataSource_ReturnsEmptyResults()
    {
        // Arrange
        var request = new AutocompleteRequest("test", 5, "invalid");

        // Act
        var result = await _autocompleteService.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Query.Should().Be("test");
        result.DataSource.Should().Be("invalid");
        result.Results.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_WhenDataSourceThrows_ReturnsEmptyResults()
    {
        // Arrange
        var request = new AutocompleteRequest("test", 5, "cities");

        _citiesDataSourceMock
            .Setup(x => x.SearchAsync("test", 5))
            .ThrowsAsync(new Exception("Data source error"));

        // Act
        var result = await _autocompleteService.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchCitiesAsync_CallsSearchAsyncWithCitiesDataSource()
    {
        // Arrange
        var query = "Berlin";
        var limit = 10;
        var expectedResults = new List<AutocompleteItem>
        {
            new("berlin_de", "Berlin, DE", "Berlin", new Dictionary<string, object> { ["country"] = "DE" })
        };

        _citiesDataSourceMock
            .Setup(x => x.SearchAsync(query, limit))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _autocompleteService.SearchCitiesAsync(query, limit);

        // Assert
        result.Should().NotBeNull();
        result.DataSource.Should().Be("cities");
        result.Results.Should().HaveCount(1);
        result.Results.First().Value.Should().Be("Berlin");
    }

    [Fact]
    public async Task GetAvailableDataSourcesAsync_ReturnsAllDataSourceNames()
    {
        // Act
        var result = await _autocompleteService.GetAvailableDataSourcesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain("cities");
        result.Should().Contain("countries");
    }

    [Theory]
    [InlineData("cities", true)]
    [InlineData("countries", true)]
    [InlineData("invalid", false)]
    public async Task IsDataSourceAvailableAsync_ReturnsCorrectAvailability(string dataSource, bool expected)
    {
        // Act
        var result = await _autocompleteService.IsDataSourceAvailableAsync(dataSource);

        // Assert
        result.Should().Be(expected);
    }


} 