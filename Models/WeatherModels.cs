using System.Text.Json.Serialization;

namespace DotnetLearningShowcase.Models;

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public record ForecastDto(DateOnly Date, int TemperatureC, string Summary);

public record WeatherSaveRequest(string City, string Country, List<ForecastDto> Forecasts);

public record WeatherForecastRecord(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("date")] DateTime Date,
    [property: JsonPropertyName("temperatureC")] int TemperatureC,
    [property: JsonPropertyName("summary")] string Summary
);

public record UpdateSummaryRequest(string Summary);

public record WeatherApiResponse(string Location, WeatherForecast[] Forecasts);

public record LocationData(double Latitude, double Longitude, string Name, string Country);

public record BulkWeatherRequest(List<string> Cities);

// New models for hourly weather forecasts
public record HourlyWeatherForecast(
    DateTime Time,
    double TemperatureC,
    double Precipitation,
    int WeatherCode,
    double? WindSpeed = null,
    double? Humidity = null,
    double? ApparentTemperature = null
)
{
    public double TemperatureF => 32 + (TemperatureC * 9 / 5);
    public string WeatherDescription => GetWeatherDescription(WeatherCode);
    
    private static string GetWeatherDescription(int code) => code switch
    {
        0 => "Clear sky",
        1 => "Mainly clear",
        2 => "Partly cloudy",
        3 => "Overcast",
        45 or 48 => "Foggy",
        51 or 53 or 55 => "Drizzle",
        61 or 63 or 65 => "Rain",
        71 or 73 or 75 => "Snow",
        80 or 81 or 82 => "Rain showers",
        95 => "Thunderstorm",
        _ => "Unknown"
    };
}

public record HourlyWeatherResponse(
    string Location,
    HourlyWeatherForecast[] HourlyForecasts
); 