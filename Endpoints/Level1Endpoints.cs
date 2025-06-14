using DotnetLearningShowcase.Models;
using DotnetLearningShowcase.Services;

namespace DotnetLearningShowcase.Endpoints;

public static class Level1Endpoints
{
    public static void MapLevel1Endpoints(this IEndpointRouteBuilder app)
    {
        var level1 = app.MapGroup("/level1");

        level1.MapGet("/", () => "Level 1 - Basic Weather API");

        level1.MapGet("/weatherforecast", async (IWeatherService weatherService, string? city = "Berlin") =>
        {
            var result = await weatherService.GetWeatherForecastAsync(city);
            
            if (result == null)
            {
                return Results.NotFound($"City '{city}' not found.");
            }

            return Results.Ok(result);
        })
        .WithName("Level1_GetWeatherForecast");

        level1.MapGet("/weatherforecast/hourly", async (IWeatherService weatherService, string? city = "Berlin") =>
        {
            var result = await weatherService.GetHourlyWeatherComparisonAsync(city);
            
            if (result == null)
            {
                return Results.NotFound($"City '{city}' not found.");
            }

            return Results.Ok(result);
        })
        .WithName("Level1_GetHourlyWeatherForecast")
        .WithSummary("Get hourly weather forecast for today with yesterday comparison")
        .WithDescription("Returns hourly weather data for today including temperature, precipitation, weather conditions, wind speed, humidity, and comparison with yesterday's weather for temperature analysis");

        // Enhanced endpoint with city suggestions for invalid cities
        level1.MapGet("/weatherforecast/smart", async (
            IWeatherService weatherService, 
            IAutocompleteService autocompleteService, 
            string? city = "Berlin") =>
        {
            var result = await weatherService.GetWeatherForecastAsync(city);
            
            if (result != null)
            {
                return Results.Ok(new
                {
                    weather = result,
                    cityFound = true,
                    suggestions = (List<AutocompleteItem>?)null
                });
            }

            // If city not found, provide autocomplete suggestions
            var suggestions = await autocompleteService.SearchCitiesAsync(city ?? "", 5);
            
            return Results.NotFound(new
            {
                message = $"City '{city}' not found.",
                cityFound = false,
                suggestions = suggestions.Results,
                hint = "Try one of the suggested cities or use /level3/cities/autocomplete for more options"
            });
        })
        .WithName("Level1_SmartWeatherForecast")
        .WithSummary("Weather forecast with intelligent city suggestions when city not found");

        level1.MapGet("/weatherforecast/hourly/smart", async (
            IWeatherService weatherService, 
            IAutocompleteService autocompleteService, 
            string? city = "Berlin") =>
        {
            var result = await weatherService.GetHourlyWeatherComparisonAsync(city);
            
            if (result != null)
            {
                return Results.Ok(new
                {
                    weather = result,
                    cityFound = true,
                    suggestions = (List<AutocompleteItem>?)null
                });
            }

            // If city not found, provide autocomplete suggestions
            var suggestions = await autocompleteService.SearchCitiesAsync(city ?? "", 5);
            
            return Results.NotFound(new
            {
                message = $"City '{city}' not found.",
                cityFound = false,
                suggestions = suggestions.Results,
                hint = "Try one of the suggested cities or use /level3/cities/autocomplete for more options"
            });
        })
        .WithName("Level1_SmartHourlyWeatherForecast")
        .WithSummary("Hourly weather forecast with intelligent city suggestions when city not found");
    }
} 