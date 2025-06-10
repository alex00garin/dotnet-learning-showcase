using System.Text.Json.Serialization;

namespace DotnetLearningShowcase.Models;

// Generic autocomplete models - reusable for cities, countries, products, etc.
public record AutocompleteRequest(
    [property: JsonPropertyName("query")] string Query,
    [property: JsonPropertyName("limit")] int Limit = 10,
    [property: JsonPropertyName("dataSource")] string DataSource = "cities"
);

public record AutocompleteItem(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("metadata")] Dictionary<string, object>? Metadata = null
);

public record AutocompleteResponse(
    [property: JsonPropertyName("query")] string Query,
    [property: JsonPropertyName("results")] List<AutocompleteItem> Results,
    [property: JsonPropertyName("totalCount")] int TotalCount,
    [property: JsonPropertyName("dataSource")] string DataSource
);

// City-specific models
public record CityData(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("lat")] string Latitude,
    [property: JsonPropertyName("lng")] string Longitude,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("admin1")] string? Admin1 = null,
    [property: JsonPropertyName("admin2")] string? Admin2 = null
);

// Generic data source interface
public interface IAutocompleteDataSource<T>
{
    string Name { get; }
    Task<List<T>> GetAllDataAsync();
    Task<List<AutocompleteItem>> SearchAsync(string query, int limit);
} 