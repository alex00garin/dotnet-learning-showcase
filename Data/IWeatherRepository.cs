using DotnetLearningShowcase.Models;

namespace DotnetLearningShowcase.Data;

public interface IWeatherRepository
{
    Task InitializeDatabaseAsync();
    Task SaveForecastsAsync(string city, string country, IEnumerable<ForecastDto> forecasts);
    Task<IEnumerable<WeatherForecastRecord>> GetHistoryAsync(string city);
    Task<IEnumerable<WeatherForecastRecord>> GetAllRecordsAsync();
    Task<bool> UpdateSummaryAsync(Guid id, string summary);
    Task<bool> DeleteForecastAsync(Guid id);
} 