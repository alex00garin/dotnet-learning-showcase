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
    }
} 