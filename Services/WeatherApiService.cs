using System.Text.Json;
using DotnetLearningShowcase.Models;

namespace DotnetLearningShowcase.Services;

public class WeatherApiService : IWeatherApiService
{
    private readonly HttpClient _httpClient;

    public WeatherApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WeatherForecast[]> GetForecastsAsync(LocationData location)
    {
        try
        {
            var weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={location.Latitude}&longitude={location.Longitude}&daily=temperature_2m_max,temperature_2m_min,precipitation_probability_max&timezone=auto";
            var weatherResponse = await _httpClient.GetStringAsync(weatherUrl);

            using var weatherDoc = JsonDocument.Parse(weatherResponse);
            var daily = weatherDoc.RootElement.GetProperty("daily");

            var dates = daily.GetProperty("time").EnumerateArray();
            var temps = daily.GetProperty("temperature_2m_max").EnumerateArray();
            var precip = daily.GetProperty("precipitation_probability_max").EnumerateArray();

            var forecasts = dates.Zip(
                temps.Zip(precip, (temp, prob) => (temp.GetDouble(), prob.GetDouble())),
                (date, data) =>
                {
                    var (temp, prob) = data;
                    var summary = GenerateSummary(temp, prob, location);

                    return new WeatherForecast(
                        DateOnly.Parse(date.GetString() ?? DateTime.UtcNow.ToString("yyyy-MM-dd")),
                        (int)temp,
                        summary
                    );
                }).ToArray();

            return forecasts;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting weather forecast for {location.Name}: {ex.Message}");
            throw;
        }
    }

    private static string GenerateSummary(double temp, double prob, LocationData location)
    {
        var weather = prob > 50 ? "Rainy" :
                      temp > 25 ? "Hot" :
                      temp > 20 ? "Warm" :
                      temp > 15 ? "Mild" :
                      temp > 10 ? "Cool" :
                      temp > 5 ? "Chilly" : "Cold";

        return $"{weather} in {location.Name}, {location.Country}";
    }
} 