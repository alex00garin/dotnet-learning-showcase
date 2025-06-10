using DotnetLearningShowcase.Models;
using DotnetLearningShowcase.Services;

namespace DotnetLearningShowcase.Endpoints;

public static class Level3Endpoints
{
    public static void MapLevel3Endpoints(this IEndpointRouteBuilder app)
    {
        var level3 = app.MapGroup("/level3");

        level3.MapGet("/", () => "Level 3 - Generic Autocomplete Service");

        // Generic autocomplete endpoint - can work with any data source
        level3.MapPost("/autocomplete", async (IAutocompleteService autocompleteService, AutocompleteRequest request) =>
        {
            var result = await autocompleteService.SearchAsync(request);
            return Results.Ok(result);
        })
        .WithName("Level3_GenericAutocomplete")
        .WithSummary("Generic autocomplete that works with multiple data sources");

        // Convenient city autocomplete endpoint
        level3.MapGet("/cities/autocomplete", async (IAutocompleteService autocompleteService, string query, int limit = 10) =>
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Results.BadRequest("Query parameter is required");
            }

            var result = await autocompleteService.SearchCitiesAsync(query, limit);
            return Results.Ok(result);
        })
        .WithName("Level3_CityAutocomplete")
        .WithSummary("City autocomplete with smart search and ranking");

        // Get available data sources
        level3.MapGet("/datasources", async (IAutocompleteService autocompleteService) =>
        {
            var dataSources = await autocompleteService.GetAvailableDataSourcesAsync();
            return Results.Ok(new { dataSources });
        })
        .WithName("Level3_GetDataSources")
        .WithSummary("Get list of available autocomplete data sources");

        // Health check for specific data source
        level3.MapGet("/datasources/{dataSource}/health", async (IAutocompleteService autocompleteService, string dataSource) =>
        {
            var isAvailable = await autocompleteService.IsDataSourceAvailableAsync(dataSource);
            return Results.Ok(new { dataSource, isAvailable, status = isAvailable ? "healthy" : "unavailable" });
        })
        .WithName("Level3_DataSourceHealth")
        .WithSummary("Check if a specific data source is available and healthy");
    }
} 