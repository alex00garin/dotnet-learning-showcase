using DotnetLearningShowcase.Models;
using DotnetLearningShowcase.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotnetLearningShowcase.Tests.Services;

public class PaginationServiceTests
{
    private readonly Mock<IWeatherService> _weatherServiceMock;
    private readonly Mock<IAutocompleteService> _autocompleteServiceMock;
    private readonly PaginationService _paginationService;

    public PaginationServiceTests()
    {
        _weatherServiceMock = new Mock<IWeatherService>();
        _autocompleteServiceMock = new Mock<IAutocompleteService>();
        _paginationService = new PaginationService(_weatherServiceMock.Object, _autocompleteServiceMock.Object);
    }

    [Fact]
    public void PaginateCollection_ReturnsCorrectPaginationMetadata()
    {
        // Arrange
        var items = Enumerable.Range(1, 157).Select(i => $"Item {i}").ToList();
        var request = new PaginationRequest(2, 10);

        // Act
        var result = _paginationService.PaginateCollection(items, request);

        // Assert
        result.Should().NotBeNull();
        result.Pagination.CurrentPage.Should().Be(2);
        result.Pagination.PageSize.Should().Be(10);
        result.Pagination.TotalItems.Should().Be(157);
        result.Pagination.TotalPages.Should().Be(16);
        result.Pagination.HasNextPage.Should().BeTrue();
        result.Pagination.HasPreviousPage.Should().BeTrue();
        result.Pagination.FirstItemIndex.Should().Be(11);
        result.Pagination.LastItemIndex.Should().Be(20);
        result.Data.Count().Should().Be(10);
    }

    [Fact]
    public void PaginateCollection_HandlesLastPage()
    {
        // Arrange
        var items = Enumerable.Range(1, 25).Select(i => $"Item {i}").ToList();
        var request = new PaginationRequest(3, 10); // Last page with 5 items

        // Act
        var result = _paginationService.PaginateCollection(items, request);

        // Assert
        result.Pagination.CurrentPage.Should().Be(3);
        result.Pagination.TotalPages.Should().Be(3);
        result.Pagination.HasNextPage.Should().BeFalse();
        result.Pagination.HasPreviousPage.Should().BeTrue();
        result.Pagination.FirstItemIndex.Should().Be(21);
        result.Pagination.LastItemIndex.Should().Be(25);
        result.Data.Count().Should().Be(5);
    }

    [Fact]
    public void PaginateCollection_HandlesEmptyCollection()
    {
        // Arrange
        var items = new List<string>();
        var request = new PaginationRequest(1, 10);

        // Act
        var result = _paginationService.PaginateCollection(items, request);

        // Assert
        result.Pagination.TotalItems.Should().Be(0);
        result.Pagination.TotalPages.Should().Be(0);
        result.Pagination.HasNextPage.Should().BeFalse();
        result.Pagination.HasPreviousPage.Should().BeFalse();
        result.Pagination.FirstItemIndex.Should().Be(0);
        result.Pagination.LastItemIndex.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public void PaginationRequest_ValidatesInputs()
    {
        // Arrange & Act
        var request = new PaginationRequest(-5, 200);

        // Assert
        request.ValidatedPage.Should().Be(1); // Negative page becomes 1
        request.ValidatedPageSize.Should().Be(100); // Size clamped to max 100
        request.Skip.Should().Be(0); // (1-1) * 100 = 0
    }

    [Fact]
    public async Task GetPaginatedWeatherHistoryAsync_AppliesFiltersCorrectly()
    {
        // Arrange
        var mockHistory = new List<WeatherForecastRecord>
        {
            new(Guid.NewGuid(), "TestCity", "TestCountry", DateTime.Now.AddDays(-5), 20, "Sunny"),
            new(Guid.NewGuid(), "TestCity", "TestCountry", DateTime.Now.AddDays(-4), 15, "Cloudy"),
            new(Guid.NewGuid(), "TestCity", "TestCountry", DateTime.Now.AddDays(-3), 25, "Hot"),
            new(Guid.NewGuid(), "TestCity", "TestCountry", DateTime.Now.AddDays(-2), 10, "Cold")
        };

        _weatherServiceMock.Setup(x => x.GetHistoryAsync("TestCity"))
            .ReturnsAsync(mockHistory);

        var request = new WeatherHistoryPaginationRequest(
            "TestCity", 1, 10, "temperature", SortDirection.Descending, 
            MinTemperature: 15);

        // Act
        var result = await _paginationService.GetPaginatedWeatherHistoryAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Count().Should().Be(3); // Only items with temp >= 15
        result.Data.First().TemperatureC.Should().Be(25); // Sorted by temp descending
        result.Pagination.TotalItems.Should().Be(3);
    }

    [Fact]
    public async Task GetPaginatedAutocompleteAsync_WithCountryFilter()
    {
        // Arrange
        var mockItems = new List<AutocompleteItem>
        {
            new("1", "London", "London", new Dictionary<string, object> { ["country"] = "GB" }),
            new("2", "Los Angeles", "Los Angeles", new Dictionary<string, object> { ["country"] = "US" }),
            new("3", "Lyon", "Lyon", new Dictionary<string, object> { ["country"] = "FR" })
        };

        var mockResponse = new AutocompleteResponse("Lon", mockItems, 3, "cities");

        _autocompleteServiceMock.Setup(x => x.SearchAsync(It.IsAny<AutocompleteRequest>()))
            .ReturnsAsync(mockResponse);

        var request = new AutocompletePaginationRequest(
            "Lon", "cities", 1, 10, "name", SortDirection.Ascending, 
            CountryFilter: "GB");

        // Act
        var result = await _paginationService.GetPaginatedAutocompleteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Count().Should().Be(1); // Only London (GB)
        result.Data.First().Value.Should().Be("London");
        result.Pagination.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task GetBulkWeatherWithPaginationAsync_ProcessesBatches()
    {
        // Arrange
        var cities = new[] { "London", "Paris", "Berlin" };
        var request = new BulkOperationRequest<string>(cities, 2, true);

        _weatherServiceMock.Setup(x => x.GetWeatherForecastAsync("London"))
            .ReturnsAsync(new WeatherApiResponse("London", Array.Empty<WeatherForecast>()));
        _weatherServiceMock.Setup(x => x.GetWeatherForecastAsync("Paris"))
            .ReturnsAsync(new WeatherApiResponse("Paris", Array.Empty<WeatherForecast>()));
        _weatherServiceMock.Setup(x => x.GetWeatherForecastAsync("Berlin"))
            .ReturnsAsync((WeatherApiResponse?)null); // Simulate failure

        _autocompleteServiceMock.Setup(x => x.SearchCitiesAsync("Berlin", 1))
            .ReturnsAsync(new AutocompleteResponse("Berlin", new List<AutocompleteItem>(), 0, "cities"));

        // Act
        var result = await _paginationService.GetBulkWeatherWithPaginationAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Results.Count().Should().Be(3);
        result.SuccessCount.Should().Be(2);
        result.FailureCount.Should().Be(1);
        result.BatchInfo.Should().NotBeNull();
        result.BatchInfo!.BatchSize.Should().Be(2);
        result.BatchInfo.TotalBatches.Should().Be(2); // 3 items / 2 = 2 batches
    }

    [Theory]
    [InlineData(1, 10, 1, 10)]
    [InlineData(0, 10, 1, 10)] // Page 0 -> 1
    [InlineData(-5, 10, 1, 10)] // Negative page -> 1
    [InlineData(1, 0, 1, 1)] // Size 0 -> 1
    [InlineData(1, 150, 1, 100)] // Size > 100 -> 100
    [InlineData(999, 50, 999, 50)] // High page number preserved
    public void PaginationRequest_ValidationBehavior(int inputPage, int inputSize, int expectedPage, int expectedSize)
    {
        // Arrange & Act
        var request = new PaginationRequest(inputPage, inputSize);

        // Assert
        request.ValidatedPage.Should().Be(expectedPage);
        request.ValidatedPageSize.Should().Be(expectedSize);
    }

    [Fact]
    public void PaginationHelper_CreateResponse_CalculatesCorrectIndices()
    {
        // Arrange
        var data = Enumerable.Range(1, 5).ToList();
        var request = new PaginationRequest(3, 10);
        var totalItems = 47;

        // Act
        var result = PaginationHelper.CreateResponse(data, totalItems, request);

        // Assert
        result.Pagination.CurrentPage.Should().Be(3);
        result.Pagination.PageSize.Should().Be(10);
        result.Pagination.TotalItems.Should().Be(47);
        result.Pagination.TotalPages.Should().Be(5);
        result.Pagination.FirstItemIndex.Should().Be(21); // (3-1) * 10 + 1
        result.Pagination.LastItemIndex.Should().Be(30); // min(3 * 10, 47) = min(30, 47) = 30
        result.Pagination.HasPreviousPage.Should().BeTrue();
        result.Pagination.HasNextPage.Should().BeTrue();
    }
} 