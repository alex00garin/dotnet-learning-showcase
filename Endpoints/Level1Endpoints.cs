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
    }
} 