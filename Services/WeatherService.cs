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