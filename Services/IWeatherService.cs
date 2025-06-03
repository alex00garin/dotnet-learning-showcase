using DotnetLearningShowcase.Models;

namespace DotnetLearningShowcase.Services;

public interface IWeatherService
{
    Task<WeatherApiResponse?> GetWeatherForecastAsync(string? city = "Berlin");
    Task SaveForecastAsync(WeatherSaveRequest request);
    Task<IEnumerable<WeatherForecastRecord>> GetHistoryAsync(string city);
    Task<bool> UpdateSummaryAsync(Guid id, string summary);
    Task<bool> DeleteForecastAsync(Guid id);
}

public interface IGeocodingService
{
    Task<LocationData?> GetLocationAsync(string city);
}

public interface IWeatherApiService
{
    Task<WeatherForecast[]> GetForecastsAsync(LocationData location);
} 