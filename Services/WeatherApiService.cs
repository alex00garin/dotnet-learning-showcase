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

    public async Task<HourlyWeatherForecast[]> GetHourlyForecastsAsync(LocationData location)
    {
        try
        {
            // Request hourly data for today only (forecast_days=1)
            var weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={location.Latitude}&longitude={location.Longitude}&hourly=temperature_2m,precipitation,weather_code,wind_speed_10m,relative_humidity_2m,apparent_temperature&timezone=auto&forecast_days=1";
            var weatherResponse = await _httpClient.GetStringAsync(weatherUrl);

            using var weatherDoc = JsonDocument.Parse(weatherResponse);
            var hourly = weatherDoc.RootElement.GetProperty("hourly");

            var times = hourly.GetProperty("time").EnumerateArray();
            var temperatures = hourly.GetProperty("temperature_2m").EnumerateArray();
            var precipitation = hourly.GetProperty("precipitation").EnumerateArray();
            var weatherCodes = hourly.GetProperty("weather_code").EnumerateArray();
            var windSpeeds = hourly.GetProperty("wind_speed_10m").EnumerateArray();
            var humidity = hourly.GetProperty("relative_humidity_2m").EnumerateArray();
            var apparentTemp = hourly.GetProperty("apparent_temperature").EnumerateArray();

            var hourlyForecasts = times.Zip(
                temperatures.Zip(precipitation.Zip(weatherCodes.Zip(windSpeeds.Zip(humidity.Zip(apparentTemp, 
                    (h, at) => (h.GetDouble(), at.GetDouble())),
                    (ws, data) => (ws.GetDouble(), data.Item1, data.Item2)),
                    (wc, data) => (wc.GetInt32(), data.Item1, data.Item2, data.Item3)),
                    (p, data) => (p.GetDouble(), data.Item1, data.Item2, data.Item3, data.Item4)),
                    (t, data) => (t.GetDouble(), data.Item1, data.Item2, data.Item3, data.Item4, data.Item5)),
                (time, data) =>
                {
                    var (temp, precip, weatherCode, windSpeed, humid, apparentTemperature) = data;
                    
                    return new HourlyWeatherForecast(
                        DateTime.Parse(time.GetString() ?? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm")),
                        temp,
                        precip,
                        weatherCode,
                        windSpeed,
                        humid,
                        apparentTemperature
                    );
                }).ToArray();

            return hourlyForecasts;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting hourly weather forecast for {location.Name}: {ex.Message}");
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