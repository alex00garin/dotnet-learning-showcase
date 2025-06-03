using System.Text.Json;
using DotnetLearningShowcase.Models;

namespace DotnetLearningShowcase.Services;

public class GeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;

    public GeocodingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LocationData?> GetLocationAsync(string city)
    {
        try
        {
            var geocodingUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(city)}&count=1";
            var geocodingResponse = await _httpClient.GetStringAsync(geocodingUrl);

            using var geocodingDoc = JsonDocument.Parse(geocodingResponse);
            
            // Check if results property exists and has elements
            if (!geocodingDoc.RootElement.TryGetProperty("results", out var results) || 
                results.GetArrayLength() == 0)
            {
                return null;
            }

            var location = results[0];
            var latitude = location.GetProperty("latitude").GetDouble();
            var longitude = location.GetProperty("longitude").GetDouble();
            var cityName = location.GetProperty("name").GetString() ?? city;
            var country = location.GetProperty("country").GetString() ?? "Unknown";

            return new LocationData(latitude, longitude, cityName, country);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting location for city '{city}': {ex.Message}");
            return null; // Return null instead of throwing
        }
    }
} 