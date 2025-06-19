using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace DotnetLearningShowcase.Models;

[Table("mock_weather_data")]
public class MockWeatherData : BaseModel
{
    [PrimaryKey("id")]
    public string Id { get; set; } = string.Empty;

    [Column("city")]
    public string City { get; set; } = string.Empty;

    [Column("country")]
    public string Country { get; set; } = string.Empty;

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("temperature_c")]
    public string TemperatureC { get; set; } = string.Empty;

    [Column("summary")]
    public string Summary { get; set; } = string.Empty;
}

// Request model for Supabase weather pagination
public record SupabaseWeatherPaginationRequest : PaginationRequest
{
    public string? City { get; init; }
    public string? Country { get; init; }
    public string? Summary { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public int? MinTemperature { get; init; }
    public int? MaxTemperature { get; init; }
} 