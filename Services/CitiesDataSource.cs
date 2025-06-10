using System.Text.Json;
using DotnetLearningShowcase.Models;

namespace DotnetLearningShowcase.Services;

/// <summary>
/// Provides city autocomplete functionality using local cities.json file.
/// Contains 154,000+ cities worldwide for offline use.
/// </summary>

public class CitiesDataSource : IAutocompleteDataSource<object>
{
    private readonly ILogger<CitiesDataSource> _logger;
    private readonly string _citiesFilePath = Path.Combine("Data", "cities.json");
    private List<CityData>? _cachedCities;

    public string Name => "cities";

    public CitiesDataSource(IHttpClientFactory httpClientFactory, ILogger<CitiesDataSource> logger)
    {
        _logger = logger;
    }

    public async Task<List<object>> GetAllDataAsync()
    {
        var cities = await LoadCitiesAsync();
        return cities.Cast<object>().ToList();
    }

    public async Task<List<AutocompleteItem>> SearchAsync(string query, int limit)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<AutocompleteItem>();
        }

        var cities = await LoadCitiesAsync();
        var normalizedQuery = query.Trim().ToLowerInvariant();

        var results = cities
            .Where(city => city.Name.ToLowerInvariant().Contains(normalizedQuery))
            .OrderBy(city => city.Name.ToLowerInvariant().IndexOf(normalizedQuery))
            .ThenBy(city => city.Name.Length)
            .Take(limit)
            .Select(city => new AutocompleteItem(
                Id: $"{city.Name}_{city.Country}",
                Label: FormatCityLabel(city),
                Value: city.Name,
                Metadata: new Dictionary<string, object>
                {
                    ["country"] = city.Country,
                    ["latitude"] = city.Latitude,
                    ["longitude"] = city.Longitude,
                    ["admin1"] = city.Admin1 ?? "",
                    ["admin2"] = city.Admin2 ?? ""
                }
            ))
            .ToList();

        return results;
    }

    private async Task<List<CityData>> LoadCitiesAsync()
    {
        // Return cached data if already loaded
        if (_cachedCities != null)
        {
            return _cachedCities;
        }

        try
        {
            _logger.LogInformation("Loading cities data from local file: {FilePath}", _citiesFilePath);
            
            if (!File.Exists(_citiesFilePath))
            {
                _logger.LogError("Cities file not found at: {FilePath}", _citiesFilePath);
                return new List<CityData>();
            }

            var jsonContent = await File.ReadAllTextAsync(_citiesFilePath);
            var cities = JsonSerializer.Deserialize<List<CityData>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (cities != null)
            {
                _cachedCities = cities;
                _logger.LogInformation("Successfully loaded {Count} cities from local file", cities.Count);
            }

            return _cachedCities ?? new List<CityData>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load cities data from local file: {FilePath}", _citiesFilePath);
            return new List<CityData>();
        }
    }

    private static string FormatCityLabel(CityData city)
    {
        var label = city.Name;
        
        if (!string.IsNullOrEmpty(city.Admin1))
        {
            label += $", {city.Admin1}";
        }
        
        label += $", {city.Country}";
        
        return label;
    }
} 