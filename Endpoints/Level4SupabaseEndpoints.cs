using DotnetLearningShowcase.Models;
using DotnetLearningShowcase.Services;
using DotnetLearningShowcase.Data;
using System.Text.Json.Serialization;

#pragma warning disable CS0657 // 'property' is not a valid attribute location for this declaration

namespace DotnetLearningShowcase.Endpoints;

public static class Level4WeatherDataEndpoints
{
    public static void MapLevel4WeatherDataEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/level4/weather-data")
            .WithTags("Level 4 - Weather Data Integration");

        // Enhanced weather data pagination - shows all 100 records from Supabase mock_weather_data table
        group.MapPost("/paginated", async (
            WeatherDataPaginationRequest request,
            IWeatherRepository weatherRepository,
            IPaginationService paginationService) =>
        {
            // Get all records from the mock_weather_data table
            var allRecords = await weatherRepository.GetAllRecordsAsync();
            
            // Apply filtering if specified
            var filteredRecords = allRecords.AsEnumerable();
            
            if (!string.IsNullOrEmpty(request.City))
                filteredRecords = filteredRecords.Where(r => r.City.Contains(request.City, StringComparison.OrdinalIgnoreCase));
                
            if (!string.IsNullOrEmpty(request.Country))
                filteredRecords = filteredRecords.Where(r => r.Country.Contains(request.Country, StringComparison.OrdinalIgnoreCase));
                
            if (!string.IsNullOrEmpty(request.Summary))
                filteredRecords = filteredRecords.Where(r => r.Summary.Contains(request.Summary, StringComparison.OrdinalIgnoreCase));
                
            if (request.DateFrom.HasValue)
                filteredRecords = filteredRecords.Where(r => r.Date >= request.DateFrom.Value);
                
            if (request.DateTo.HasValue)
                filteredRecords = filteredRecords.Where(r => r.Date <= request.DateTo.Value);
                
            if (request.MinTemperature.HasValue)
                filteredRecords = filteredRecords.Where(r => r.TemperatureC >= request.MinTemperature.Value);
                
            if (request.MaxTemperature.HasValue)
                filteredRecords = filteredRecords.Where(r => r.TemperatureC <= request.MaxTemperature.Value);

            // Apply sorting
            var sortedRecords = ApplyWeatherDataSorting(filteredRecords, request.SortBy ?? "date", request.SortDirection);
            
            // Use the universal pagination service to paginate the results
            var result = paginationService.PaginateCollection(sortedRecords, request);
            
            return Results.Ok(result);
        })
        .WithName("GetPaginatedWeatherData")
        .WithSummary("Paginate all weather data from Supabase mock_weather_data table (100 records)");

        // Weather data information endpoint
        group.MapGet("/info", () =>
        {
            var info = new
            {
                Description = "Real weather data pagination endpoint showing all 100 records from Supabase",
                DataSource = "Supabase mock_weather_data table with 100 weather records",
                RequiredParameter = "none - shows all records by default",
                AvailableFilters = new[] { "city", "country", "summary", "dateFrom", "dateTo", "minTemperature", "maxTemperature" },
                AvailableSorts = new[] { "city", "country", "date", "temperatureC", "summary" },
                DefaultPageSize = 10,
                MaxPageSize = 100,
                TotalRecords = 100,
                Usage = new
                {
                    EndpointUrl = "/level4/weather-data/paginated",
                    Method = "POST",
                    ExampleRequest = new
                    {
                        page = 1,
                        pageSize = 10,
                        sortBy = "date",
                        sortDirection = "Descending"
                    }
                }
            };

            return Results.Ok(info);
        })
        .WithName("GetWeatherDataInfo")
        .WithSummary("Get information about the weather data pagination endpoint");
    }

    // Helper method for sorting weather data records
    private static IEnumerable<WeatherForecastRecord> ApplyWeatherDataSorting(
        IEnumerable<WeatherForecastRecord> records,
        string sortBy,
        SortDirection direction)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "date" => direction == SortDirection.Ascending 
                ? records.OrderBy(x => x.Date) 
                : records.OrderByDescending(x => x.Date),
            "temperature" or "temperaturec" => direction == SortDirection.Ascending 
                ? records.OrderBy(x => x.TemperatureC) 
                : records.OrderByDescending(x => x.TemperatureC),
            "city" => direction == SortDirection.Ascending 
                ? records.OrderBy(x => x.City) 
                : records.OrderByDescending(x => x.City),
            "country" => direction == SortDirection.Ascending 
                ? records.OrderBy(x => x.Country) 
                : records.OrderByDescending(x => x.Country),
            "summary" => direction == SortDirection.Ascending 
                ? records.OrderBy(x => x.Summary) 
                : records.OrderByDescending(x => x.Summary),
            _ => records.OrderByDescending(x => x.Date) // Default sort by date descending
        };
    }
}

// DTO for weather data responses
public class WeatherDataDto
{
    public string Id { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }
    public string Summary { get; set; } = string.Empty;
}

// Request model for weather data pagination
public record WeatherDataPaginationRequest(
    [property: JsonPropertyName("page")] int Page = 1,
    [property: JsonPropertyName("pageSize")] int PageSize = 10,
    [property: JsonPropertyName("sortBy")] string? SortBy = "date",
    [property: JsonPropertyName("sortDirection")] SortDirection SortDirection = SortDirection.Descending,
    [property: JsonPropertyName("city")] string? City = null,
    [property: JsonPropertyName("country")] string? Country = null,
    [property: JsonPropertyName("summary")] string? Summary = null,
    [property: JsonPropertyName("dateFrom")] DateTime? DateFrom = null,
    [property: JsonPropertyName("dateTo")] DateTime? DateTo = null,
    [property: JsonPropertyName("minTemperature")] int? MinTemperature = null,
    [property: JsonPropertyName("maxTemperature")] int? MaxTemperature = null
) : PaginationRequest(Page, PageSize, SortBy, SortDirection); 