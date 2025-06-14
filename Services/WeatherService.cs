using DotnetLearningShowcase.Data;
using DotnetLearningShowcase.Models;

namespace DotnetLearningShowcase.Services;

public class WeatherService : IWeatherService
{
    private readonly IGeocodingService _geocodingService;
    private readonly IWeatherApiService _weatherApiService;
    private readonly IWeatherRepository _weatherRepository;

    public WeatherService(
        IGeocodingService geocodingService,
        IWeatherApiService weatherApiService,
        IWeatherRepository weatherRepository)
    {
        _geocodingService = geocodingService;
        _weatherApiService = weatherApiService;
        _weatherRepository = weatherRepository;
    }

    public async Task<WeatherApiResponse?> GetWeatherForecastAsync(string? city = "Berlin")
    {
        var location = await _geocodingService.GetLocationAsync(city ?? "Berlin");
        if (location == null)
        {
            return null;
        }

        var forecasts = await _weatherApiService.GetForecastsAsync(location);
        
        return new WeatherApiResponse(
            $"{location.Name}, {location.Country}",
            forecasts
        );
    }

    public async Task<HourlyWeatherResponse?> GetHourlyWeatherForecastAsync(string? city = "Berlin")
    {
        var location = await _geocodingService.GetLocationAsync(city ?? "Berlin");
        if (location == null)
        {
            return null;
        }

        var hourlyForecasts = await _weatherApiService.GetHourlyForecastsAsync(location);
        
        return new HourlyWeatherResponse(
            $"{location.Name}, {location.Country}",
            hourlyForecasts
        );
    }

    public async Task<HourlyWeatherComparisonResponse?> GetHourlyWeatherComparisonAsync(string? city = "Berlin")
    {
        var location = await _geocodingService.GetLocationAsync(city ?? "Berlin");
        if (location == null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var yesterday = today.AddDays(-1);

        // Get both today's and yesterday's forecasts
        var todayTask = _weatherApiService.GetHourlyForecastsForDateAsync(location, today);
        var yesterdayTask = _weatherApiService.GetHourlyForecastsForDateAsync(location, yesterday);

        await Task.WhenAll(todayTask, yesterdayTask);

        var todayForecasts = await todayTask;
        var yesterdayForecasts = await yesterdayTask;

        // Find current hour comparison
        var currentHour = DateTime.Now.Hour;
        var todayCurrentHour = todayForecasts.FirstOrDefault(f => f.Time.Hour == currentHour);
        var yesterdayCurrentHour = yesterdayForecasts.FirstOrDefault(f => f.Time.Hour == currentHour);

        CurrentHourComparison? currentHourComparison = null;
        
        if (todayCurrentHour != null && yesterdayCurrentHour != null && 
            todayCurrentHour.TemperatureC > -900 && yesterdayCurrentHour.TemperatureC > -900)
        {
            var tempDiff = todayCurrentHour.TemperatureC - yesterdayCurrentHour.TemperatureC;
            var comparisonText = tempDiff switch
            {
                > 5 => $"Much warmer than yesterday (+{tempDiff:F1}°C)",
                > 2 => $"Warmer than yesterday (+{tempDiff:F1}°C)",
                > 0.5 => $"Slightly warmer than yesterday (+{tempDiff:F1}°C)",
                < -5 => $"Much colder than yesterday ({tempDiff:F1}°C)",
                < -2 => $"Colder than yesterday ({tempDiff:F1}°C)",
                < -0.5 => $"Slightly colder than yesterday ({tempDiff:F1}°C)",
                _ => $"Similar to yesterday ({tempDiff:F1}°C difference)"
            };

            currentHourComparison = new CurrentHourComparison(
                todayCurrentHour,
                yesterdayCurrentHour,
                comparisonText
            );
        }

        return new HourlyWeatherComparisonResponse(
            $"{location.Name}, {location.Country}",
            todayForecasts,
            yesterdayForecasts,
            currentHourComparison
        );
    }

    public async Task SaveForecastAsync(WeatherSaveRequest request)
    {
        await _weatherRepository.SaveForecastsAsync(request.City, request.Country, request.Forecasts);
    }

    public async Task<IEnumerable<WeatherForecastRecord>> GetHistoryAsync(string city)
    {
        return await _weatherRepository.GetHistoryAsync(city);
    }

    public async Task<bool> UpdateSummaryAsync(Guid id, string summary)
    {
        return await _weatherRepository.UpdateSummaryAsync(id, summary);
    }

    public async Task<bool> DeleteForecastAsync(Guid id)
    {
        return await _weatherRepository.DeleteForecastAsync(id);
    }
} 