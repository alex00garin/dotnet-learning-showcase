using DotnetLearningShowcase.Models;

namespace DotnetLearningShowcase.Services;

public interface IPaginationService
{
    // Weather history pagination
    Task<PaginatedResponse<WeatherForecastRecord>> GetPaginatedWeatherHistoryAsync(
        WeatherHistoryPaginationRequest request);

    // Enhanced autocomplete with pagination
    Task<PaginatedResponse<AutocompleteItem>> GetPaginatedAutocompleteAsync(
        AutocompletePaginationRequest request);

    // Cities list with advanced filtering and pagination
    Task<PaginatedResponse<AutocompleteItem>> GetPaginatedCitiesAsync(
        CitiesListRequest request);

    // Bulk weather operations with pagination
    Task<BulkOperationResponse<object>> GetBulkWeatherWithPaginationAsync(
        BulkOperationRequest<string> request);

    // Generic method for paginating any collection
    PaginatedResponse<T> PaginateCollection<T>(
        IEnumerable<T> items, 
        PaginationRequest request);
} 