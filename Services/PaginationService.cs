using DotnetLearningShowcase.Models;

namespace DotnetLearningShowcase.Services;

public class PaginationService : IPaginationService
{
    private readonly IWeatherService _weatherService;
    private readonly IAutocompleteService _autocompleteService;

    public PaginationService(
        IWeatherService weatherService,
        IAutocompleteService autocompleteService)
    {
        _weatherService = weatherService;
        _autocompleteService = autocompleteService;
    }

    public async Task<PaginatedResponse<WeatherForecastRecord>> GetPaginatedWeatherHistoryAsync(
        WeatherHistoryPaginationRequest request)
    {
        // Get all history for the city
        var allHistory = await _weatherService.GetHistoryAsync(request.City);
        
        // Apply filters
        var filteredHistory = allHistory.AsQueryable();

        if (request.DateFrom.HasValue)
        {
            filteredHistory = filteredHistory.Where(h => h.Date >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            filteredHistory = filteredHistory.Where(h => h.Date <= request.DateTo.Value);
        }

        if (request.MinTemperature.HasValue)
        {
            filteredHistory = filteredHistory.Where(h => h.TemperatureC >= request.MinTemperature.Value);
        }

        if (request.MaxTemperature.HasValue)
        {
            filteredHistory = filteredHistory.Where(h => h.TemperatureC <= request.MaxTemperature.Value);
        }

        // Apply sorting
        filteredHistory = ApplySorting(filteredHistory, request.SortBy ?? "date", request.SortDirection);

        var totalItems = filteredHistory.Count();
        var paginatedData = filteredHistory.ApplyPagination(request).ToList();

        return PaginationHelper.CreateResponse(paginatedData, totalItems, request);
    }

    public async Task<PaginatedResponse<AutocompleteItem>> GetPaginatedAutocompleteAsync(
        AutocompletePaginationRequest request)
    {
        // Get autocomplete results
        var autocompleteRequest = new AutocompleteRequest(
            request.Query,
            request.ValidatedPageSize * 5, // Get more results for better pagination
            request.DataSource
        );

        var results = await _autocompleteService.SearchAsync(autocompleteRequest);
        var items = results.Results.ToList();

        // Apply additional filters
        if (!string.IsNullOrWhiteSpace(request.CountryFilter))
        {
            items = items.Where(item => 
                item.Metadata != null && 
                item.Metadata.ContainsKey("country") &&
                item.Metadata["country"].ToString()!.Contains(request.CountryFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (request.MinPopulation.HasValue && request.DataSource == "cities")
        {
            items = items.Where(item => 
            {
                if (item.Metadata == null || !item.Metadata.ContainsKey("population")) 
                    return false;
                
                return int.TryParse(item.Metadata["population"].ToString(), out var population) &&
                       population >= request.MinPopulation.Value;
            }).ToList();
        }

        // Apply sorting (relevance is default, already sorted by autocomplete service)
        if (request.SortBy != "relevance")
        {
            items = ApplyAutocompleteSortingToList(items, request.SortBy, request.SortDirection);
        }

        var totalItems = items.Count;
        var paginatedData = items.ApplyPagination(request).ToList();

        return PaginationHelper.CreateResponse(paginatedData, totalItems, request);
    }

    public async Task<PaginatedResponse<AutocompleteItem>> GetPaginatedCitiesAsync(
        CitiesListRequest request)
    {
        // Start with a broad search or get all cities
        var searchQuery = string.IsNullOrWhiteSpace(request.SearchQuery) ? "a" : request.SearchQuery;
        
        var autocompleteRequest = new AutocompleteRequest(
            searchQuery,
            10000, // Get a large number to work with
            "cities"
        );

        var results = await _autocompleteService.SearchAsync(autocompleteRequest);
        var cities = results.Results.ToList();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.CountryFilter))
        {
            cities = cities.Where(city => 
                city.Metadata != null && 
                city.Metadata.ContainsKey("country") &&
                city.Metadata["country"].ToString()!.Equals(request.CountryFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            cities = cities.Where(city => 
                city.Value.Contains(request.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                city.Label.Contains(request.SearchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (request.MinPopulation.HasValue)
        {
            cities = cities.Where(city => 
            {
                if (city.Metadata == null || !city.Metadata.ContainsKey("population")) 
                    return false;
                
                return int.TryParse(city.Metadata["population"].ToString(), out var population) &&
                       population >= request.MinPopulation.Value;
            }).ToList();
        }

        if (request.MaxPopulation.HasValue)
        {
            cities = cities.Where(city => 
            {
                if (city.Metadata == null || !city.Metadata.ContainsKey("population")) 
                    return false;
                
                return int.TryParse(city.Metadata["population"].ToString(), out var population) &&
                       population <= request.MaxPopulation.Value;
            }).ToList();
        }

        // Apply sorting
        cities = ApplyAutocompleteSortingToList(cities, request.SortBy ?? "name", request.SortDirection);

        var totalItems = cities.Count;
        var paginatedData = cities.ApplyPagination(request).ToList();

        return PaginationHelper.CreateResponse(paginatedData, totalItems, request);
    }

    public async Task<BulkOperationResponse<object>> GetBulkWeatherWithPaginationAsync(
        BulkOperationRequest<string> request)
    {
        var cities = request.Items.ToList();
        var batchSize = Math.Max(1, Math.Min(request.BatchSize, 50)); // Limit batch size
        var totalBatches = (int)Math.Ceiling((double)cities.Count / batchSize);
        
        var allResults = new List<object>();
        var successCount = 0;
        var failureCount = 0;

        for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
        {
            var batch = cities.Skip(batchIndex * batchSize).Take(batchSize);
            var batchResults = new List<object>();

            foreach (var city in batch)
            {
                try
                {
                    var weather = await _weatherService.GetWeatherForecastAsync(city);
                    if (weather != null)
                    {
                        batchResults.Add(new
                        {
                            city,
                            success = true,
                            weather,
                            batchInfo = new
                            {
                                batchNumber = batchIndex + 1,
                                totalBatches,
                                itemInBatch = batchResults.Count + 1
                            }
                        });
                        successCount++;
                    }
                    else
                    {
                        // Try autocomplete fallback
                        var suggestions = await _autocompleteService.SearchCitiesAsync(city, 1);
                        batchResults.Add(new
                        {
                            city,
                            success = false,
                            error = "City not found",
                            suggestion = suggestions.Results.FirstOrDefault(),
                            batchInfo = new
                            {
                                batchNumber = batchIndex + 1,
                                totalBatches,
                                itemInBatch = batchResults.Count + 1
                            }
                        });
                        failureCount++;
                    }
                }
                catch (Exception ex)
                {
                    batchResults.Add(new
                    {
                        city,
                        success = false,
                        error = ex.Message,
                        batchInfo = new
                        {
                            batchNumber = batchIndex + 1,
                            totalBatches,
                            itemInBatch = batchResults.Count + 1
                        }
                    });
                    failureCount++;
                }
            }

            allResults.AddRange(batchResults);
        }

        var batchInfo = new BatchInfo(batchSize, totalBatches, totalBatches);
        
        return new BulkOperationResponse<object>(
            allResults,
            successCount,
            failureCount,
            request.IncludePagination ? batchInfo : null
        );
    }

    public PaginatedResponse<T> PaginateCollection<T>(
        IEnumerable<T> items,
        PaginationRequest request)
    {
        var totalItems = items.Count();
        var paginatedData = items.ApplyPagination(request);

        return PaginationHelper.CreateResponse(paginatedData, totalItems, request);
    }

    private static IQueryable<WeatherForecastRecord> ApplySorting(
        IQueryable<WeatherForecastRecord> query,
        string sortBy,
        SortDirection direction)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "date" => direction == SortDirection.Ascending 
                ? query.OrderBy(x => x.Date) 
                : query.OrderByDescending(x => x.Date),
            "temperature" or "temperaturec" => direction == SortDirection.Ascending 
                ? query.OrderBy(x => x.TemperatureC) 
                : query.OrderByDescending(x => x.TemperatureC),
            "city" => direction == SortDirection.Ascending 
                ? query.OrderBy(x => x.City) 
                : query.OrderByDescending(x => x.City),
            "summary" => direction == SortDirection.Ascending 
                ? query.OrderBy(x => x.Summary) 
                : query.OrderByDescending(x => x.Summary),
            _ => query.OrderByDescending(x => x.Date) // Default sort
        };
    }

    private static List<AutocompleteItem> ApplyAutocompleteSortingToList(
        List<AutocompleteItem> items,
        string? sortBy,
        SortDirection direction)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "name" or "value" => direction == SortDirection.Ascending 
                ? items.OrderBy(x => x.Value).ToList()
                : items.OrderByDescending(x => x.Value).ToList(),
            "country" => direction == SortDirection.Ascending 
                ? items.OrderBy(x => x.Metadata != null && x.Metadata.ContainsKey("country") ? x.Metadata["country"].ToString() : "").ToList()
                : items.OrderByDescending(x => x.Metadata != null && x.Metadata.ContainsKey("country") ? x.Metadata["country"].ToString() : "").ToList(),
            "population" => direction == SortDirection.Ascending 
                ? items.OrderBy(x => 
                {
                    if (x.Metadata == null || !x.Metadata.ContainsKey("population")) return 0;
                    return int.TryParse(x.Metadata["population"].ToString(), out var pop) ? pop : 0;
                }).ToList()
                : items.OrderByDescending(x => 
                {
                    if (x.Metadata == null || !x.Metadata.ContainsKey("population")) return 0;
                    return int.TryParse(x.Metadata["population"].ToString(), out var pop) ? pop : 0;
                }).ToList(),
            _ => items // Keep original order (relevance-based)
        };
    }
} 