using System.Text.Json.Serialization;

namespace DotnetLearningShowcase.Models;

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public record ForecastDto(DateOnly Date, int TemperatureC, string Summary);

public record WeatherSaveRequest(string City, string Country, List<ForecastDto> Forecasts);

public record WeatherForecastRecord(
    Guid Id,
    string City,
    string Country,
    DateTime Date,
    int TemperatureC,
    string Summary
);

public record UpdateSummaryRequest(string Summary);

public record WeatherApiResponse(string Location, WeatherForecast[] Forecasts);

public record LocationData(double Latitude, double Longitude, string Name, string Country); 