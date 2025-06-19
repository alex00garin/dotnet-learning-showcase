using System.Text.Json;
using System.Text.Json.Serialization;
using DotnetLearningShowcase.Models;
using DotnetLearningShowcase.Services;

namespace DotnetLearningShowcase.Endpoints;

public static class Level4Endpoints
{
    public static void MapLevel4Endpoints(this IEndpointRouteBuilder app)
    {
        var level4 = app.MapGroup("/level4");

        level4.MapGet("/", () => "Level 4 - Advanced Pagination & Data Management");

        // Paginated weather history with filtering and sorting
        level4.MapPost("/weather/history/paginated", async (
            IPaginationService paginationService,
            WeatherHistoryPaginationRequest request) =>
        {
            var result = await paginationService.GetPaginatedWeatherHistoryAsync(request);
            return Results.Ok(result);
        })
        .WithName("Level4_PaginatedWeatherHistory")
        .WithSummary("Paginated weather history with advanced filtering and sorting")
        .WithDescription("Get weather history with pagination, date/temperature filtering, and flexible sorting");

        // Enhanced autocomplete with pagination and filtering
        level4.MapPost("/autocomplete/paginated", async (
            IPaginationService paginationService,
            AutocompletePaginationRequest request) =>
        {
            var result = await paginationService.GetPaginatedAutocompleteAsync(request);
            return Results.Ok(result);
        })
        .WithName("Level4_PaginatedAutocomplete")
        .WithSummary("Enhanced autocomplete with pagination and filtering")
        .WithDescription("Autocomplete search with pagination, country filtering, and population-based filtering");

        // Cities browse with advanced pagination
        level4.MapPost("/cities/browse", async (
            IPaginationService paginationService,
            CitiesListRequest request) =>
        {
            var result = await paginationService.GetPaginatedCitiesAsync(request);
            return Results.Ok(result);
        })
        .WithName("Level4_BrowseCities")
        .WithSummary("Browse cities with advanced pagination and filtering")
        .WithDescription("Browse all cities with pagination, country filter, population range, and search");

        // GET version for simple city browsing
        level4.MapGet("/cities/browse", async (
            IPaginationService paginationService,
            int page = 1,
            int pageSize = 20,
            string? sortBy = "name",
            string? sortDirection = "Ascending",
            string? countryFilter = null,
            string? searchQuery = null,
            int? minPopulation = null,
            int? maxPopulation = null) =>
        {
            var direction = Enum.TryParse<SortDirection>(sortDirection, true, out var parsedDirection) 
                ? parsedDirection 
                : SortDirection.Ascending;

            var request = new CitiesListRequest(
                page, pageSize, sortBy, direction, 
                countryFilter, searchQuery, minPopulation, maxPopulation);

            var result = await paginationService.GetPaginatedCitiesAsync(request);
            return Results.Ok(result);
        })
        .WithName("Level4_BrowseCitiesGet")
        .WithSummary("Simple GET endpoint for browsing cities with pagination");

        // Bulk weather operations with batch processing
        level4.MapPost("/weather/bulk/paginated", async (
            IPaginationService paginationService,
            BulkOperationRequest<string> request) =>
        {
            var result = await paginationService.GetBulkWeatherWithPaginationAsync(request);
            return Results.Ok(result);
        })
        .WithName("Level4_BulkWeatherPaginated")
        .WithSummary("Bulk weather operations with batch processing and pagination")
        .WithDescription("Process multiple cities in batches with detailed pagination metadata");



        // Pagination metadata helper endpoint
        level4.MapGet("/pagination/info", (
            int totalItems = 157,
            int page = 1,
            int pageSize = 10) =>
        {
            var request = new PaginationRequest(page, pageSize);
            var metadata = PaginationHelper.CreateResponse(
                Enumerable.Empty<object>(), 
                totalItems, 
                request).Pagination;

            return Results.Ok(new
            {
                metadata,
                explanation = new
                {
                    totalItems,
                    requestedPage = page,
                    requestedPageSize = pageSize,
                    validatedPage = request.ValidatedPage,
                    validatedPageSize = request.ValidatedPageSize,
                    calculatedSkip = request.Skip,
                    usefulQueries = new[]
                    {
                        $"/level4/pagination/info?totalItems={totalItems}&page=1&pageSize=5",
                        $"/level4/pagination/info?totalItems={totalItems}&page=10&pageSize=20",
                        $"/level4/pagination/info?totalItems={totalItems}&page=999&pageSize=10"
                    }
                }
            });
        })
        .WithName("Level4_PaginationInfo")
        .WithSummary("Pagination metadata calculator and helper")
        .WithDescription("Calculate pagination metadata for any total items count and page parameters");

        // Advanced search with multiple criteria
        level4.MapPost("/search/advanced", async (
            IPaginationService paginationService,
            IAutocompleteService autocompleteService,
            AdvancedSearchRequest request) =>
        {
            var results = new List<object>();

            // Search cities if requested
            if (request.IncludeCities)
            {
                var cityRequest = new CitiesListRequest(
                    request.Page,
                    request.PageSize / (request.IncludeCities && request.IncludeCountries ? 2 : 1),
                    request.SortBy,
                    request.SortDirection,
                    request.CountryFilter,
                    request.Query
                );

                var cityResults = await paginationService.GetPaginatedCitiesAsync(cityRequest);
                results.Add(new
                {
                    type = "cities",
                    results = cityResults
                });
            }

            // Search countries if requested
            if (request.IncludeCountries)
            {
                var countryRequest = new AutocompletePaginationRequest(
                    request.Query ?? "",
                    "countries",
                    request.Page,
                    request.PageSize / (request.IncludeCities && request.IncludeCountries ? 2 : 1),
                    request.SortBy,
                    request.SortDirection
                );

                var countryResults = await paginationService.GetPaginatedAutocompleteAsync(countryRequest);
                results.Add(new
                {
                    type = "countries", 
                    results = countryResults
                });
            }

            return Results.Ok(new
            {
                query = request.Query,
                searchTypes = new[]
                {
                    request.IncludeCities ? "cities" : null,
                    request.IncludeCountries ? "countries" : null
                }.Where(x => x != null),
                results
            });
        })
        .WithName("Level4_AdvancedSearch")
        .WithSummary("Advanced multi-type search with pagination")
        .WithDescription("Search across multiple data types (cities, countries) with unified pagination");
    }
}

// Advanced search request model
public record AdvancedSearchRequest(
    [property: JsonPropertyName("query")] string? Query = null,
    [property: JsonPropertyName("page")] int Page = 1,
    [property: JsonPropertyName("pageSize")] int PageSize = 20,
    [property: JsonPropertyName("sortBy")] string? SortBy = "relevance",
    [property: JsonPropertyName("sortDirection")] SortDirection SortDirection = SortDirection.Descending,
    [property: JsonPropertyName("includeCities")] bool IncludeCities = true,
    [property: JsonPropertyName("includeCountries")] bool IncludeCountries = false,
    [property: JsonPropertyName("countryFilter")] string? CountryFilter = null
); 