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

        // Calculate comparison metrics
        var todayAvgTemp = todayForecasts.Length > 0 ? todayForecasts.Average(f => f.TemperatureC) : 0;
        var yesterdayAvgTemp = yesterdayForecasts.Length > 0 ? yesterdayForecasts.Average(f => f.TemperatureC) : 0;
        var tempDifference = todayAvgTemp - yesterdayAvgTemp;

        var comparisonText = tempDifference switch
        {
            > 2 => $"Today is much warmer than yesterday (+{tempDifference:F1}°C)",
            > 0.5 => $"Today is warmer than yesterday (+{tempDifference:F1}°C)",
            < -2 => $"Today is much colder than yesterday ({tempDifference:F1}°C)",
            < -0.5 => $"Today is colder than yesterday ({tempDifference:F1}°C)",
            _ => $"Today's temperature is similar to yesterday ({tempDifference:F1}°C difference)"
        };

        var comparison = new WeatherComparison(
            todayAvgTemp,
            yesterdayAvgTemp,
            tempDifference,
            comparisonText
        );

        return new HourlyWeatherComparisonResponse(
            $"{location.Name}, {location.Country}",
            todayForecasts,
            yesterdayForecasts,
            comparison
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