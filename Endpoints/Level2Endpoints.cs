using System.Text.Json;
using DotnetLearningShowcase.Models;
using DotnetLearningShowcase.Services;

namespace DotnetLearningShowcase.Endpoints;

public static class Level2Endpoints
{
    public static void MapLevel2Endpoints(this IEndpointRouteBuilder app)
    {
        var level2 = app.MapGroup("/level2");

        level2.MapGet("/", () => "Level 2 - Weather CRUD");

        level2.MapPost("/weatherforecast/save", async (IWeatherService weatherService, WeatherSaveRequest request) =>
        {
            await weatherService.SaveForecastAsync(request);
            return Results.Text("Saved");
        })
        .WithName("Level2_SaveWeatherForecast");

        level2.MapGet("/weatherforecast/history/{city}", async (IWeatherService weatherService, string city) =>
        {
            var results = await weatherService.GetHistoryAsync(city);
            var json = JsonSerializer.Serialize(results);
            return Results.Text(json, "application/json");
        })
        .WithName("Level2_GetWeatherHistory");

        // Enhanced endpoint with fuzzy city matching
        level2.MapGet("/weatherforecast/history/smart/{city}", async (
            IWeatherService weatherService,
            IAutocompleteService autocompleteService,
            string city) =>
        {
            var results = await weatherService.GetHistoryAsync(city);
            
            if (results.Any())
            {
                return Results.Ok(new
                {
                    city,
                    exactMatch = true,
                    history = results,
                    suggestions = (List<AutocompleteItem>?)null
                });
            }

            // If no exact match, try to find similar cities with data
            var citysuggestions = await autocompleteService.SearchCitiesAsync(city, 10);
            var citiesWithData = new List<object>();

            // Check which suggested cities have historical data
            foreach (var suggestion in citysuggestions.Results.Take(5))
            {
                var suggestionHistory = await weatherService.GetHistoryAsync(suggestion.Value);
                if (suggestionHistory.Any())
                {
                    citiesWithData.Add(new
                    {
                        city = suggestion,
                        recordCount = suggestionHistory.Count()
                    });
                }
            }

            return Results.NotFound(new
            {
                city,
                exactMatch = false,
                message = $"No weather history found for '{city}'",
                citiesWithData,
                allSuggestions = citysuggestions.Results
            });
        })
        .WithName("Level2_SmartWeatherHistory")
        .WithSummary("Weather history with fuzzy city matching and data availability check");

        level2.MapPut("/weatherforecast/update/{id}", async (IWeatherService weatherService, Guid id, UpdateSummaryRequest request) =>
        {
            var success = await weatherService.UpdateSummaryAsync(id, request.Summary);
            return success 
                ? Results.Text("Updated") 
                : Results.Text("Not found", statusCode: 404);
        })
        .WithName("Level2_UpdateWeatherForecast");

        level2.MapDelete("/weatherforecast/delete/{id}", async (IWeatherService weatherService, Guid id) =>
        {
            var success = await weatherService.DeleteForecastAsync(id);
            return success 
                ? Results.Text("Deleted") 
                : Results.Text("Not found", statusCode: 404);
        })
        .WithName("Level2_DeleteWeatherForecast");

        // Bulk weather fetch for multiple cities using autocomplete
        level2.MapPost("/weatherforecast/bulk", async (
            IWeatherService weatherService,
            IAutocompleteService autocompleteService,
            BulkWeatherRequest request) =>
        {
            var results = new List<object>();

            foreach (var cityQuery in request.Cities)
            {
                // First try exact match
                var weather = await weatherService.GetWeatherForecastAsync(cityQuery);
                
                if (weather != null)
                {
                    results.Add(new
                    {
                        query = cityQuery,
                        matched = true,
                        weather
                    });
                }
                else
                {
                    // Try autocomplete to find closest match
                    var suggestions = await autocompleteService.SearchCitiesAsync(cityQuery, 1);
                    if (suggestions.Results.Any())
                    {
                        var bestMatch = suggestions.Results.First();
                        var weatherForBestMatch = await weatherService.GetWeatherForecastAsync(bestMatch.Value);
                        
                        results.Add(new
                        {
                            query = cityQuery,
                            matched = false,
                            suggestion = bestMatch,
                            weather = weatherForBestMatch
                        });
                    }
                    else
                    {
                        results.Add(new
                        {
                            query = cityQuery,
                            matched = false,
                            error = "No matching city found"
                        });
                    }
                }
            }

            return Results.Ok(new { results });
        })
        .WithName("Level2_BulkWeatherForecast")
        .WithSummary("Bulk weather forecast with intelligent city matching");
    }
} 