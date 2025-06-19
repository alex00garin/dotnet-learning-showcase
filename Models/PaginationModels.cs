#pragma warning disable CS0657 // 'property' is not a valid attribute location for record parameters
using System.Text.Json.Serialization;

namespace DotnetLearningShowcase.Models;

// Generic pagination request parameters
public record PaginationRequest(
    [property: JsonPropertyName("page")] int Page = 1,
    [property: JsonPropertyName("pageSize")] int PageSize = 10,
    [property: JsonPropertyName("sortBy")] string? SortBy = null,
    [property: JsonPropertyName("sortDirection")] SortDirection SortDirection = SortDirection.Ascending
)
{
    // Validation properties
    public int ValidatedPage => Math.Max(1, Page);
    public int ValidatedPageSize => Math.Clamp(PageSize, 1, 100);
    public int Skip => (ValidatedPage - 1) * ValidatedPageSize;
}

// Generic paginated response wrapper
public record PaginatedResponse<T>(
    [property: JsonPropertyName("data")] IEnumerable<T> Data,
    [property: JsonPropertyName("pagination")] PaginationMetadata Pagination
);

// Pagination metadata
public record PaginationMetadata(
    [property: JsonPropertyName("currentPage")] int CurrentPage,
    [property: JsonPropertyName("pageSize")] int PageSize,
    [property: JsonPropertyName("totalItems")] int TotalItems,
    [property: JsonPropertyName("totalPages")] int TotalPages,
    [property: JsonPropertyName("hasNextPage")] bool HasNextPage,
    [property: JsonPropertyName("hasPreviousPage")] bool HasPreviousPage,
    [property: JsonPropertyName("firstItemIndex")] int FirstItemIndex,
    [property: JsonPropertyName("lastItemIndex")] int LastItemIndex
);

// Sort direction enum
public enum SortDirection
{
    Ascending,
    Descending
}

// Weather history pagination request
public record WeatherHistoryPaginationRequest(
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("page")] int Page = 1,
    [property: JsonPropertyName("pageSize")] int PageSize = 10,
    [property: JsonPropertyName("sortBy")] string? SortBy = "date",
    [property: JsonPropertyName("sortDirection")] SortDirection SortDirection = SortDirection.Descending,
    [property: JsonPropertyName("dateFrom")] DateTime? DateFrom = null,
    [property: JsonPropertyName("dateTo")] DateTime? DateTo = null,
    [property: JsonPropertyName("minTemperature")] int? MinTemperature = null,
    [property: JsonPropertyName("maxTemperature")] int? MaxTemperature = null
) : PaginationRequest(Page, PageSize, SortBy, SortDirection);

// Enhanced autocomplete pagination request
public record AutocompletePaginationRequest(
    [property: JsonPropertyName("query")] string Query,
    [property: JsonPropertyName("dataSource")] string DataSource = "cities",
    [property: JsonPropertyName("page")] int Page = 1,
    [property: JsonPropertyName("pageSize")] int PageSize = 10,
    [property: JsonPropertyName("sortBy")] string? SortBy = "relevance",
    [property: JsonPropertyName("sortDirection")] SortDirection SortDirection = SortDirection.Descending,
    [property: JsonPropertyName("countryFilter")] string? CountryFilter = null,
    [property: JsonPropertyName("minPopulation")] int? MinPopulation = null
) : PaginationRequest(Page, PageSize, SortBy, SortDirection);

// Cities list pagination request
public record CitiesListRequest(
    [property: JsonPropertyName("page")] int Page = 1,
    [property: JsonPropertyName("pageSize")] int PageSize = 20,
    [property: JsonPropertyName("sortBy")] string? SortBy = "name",
    [property: JsonPropertyName("sortDirection")] SortDirection SortDirection = SortDirection.Ascending,
    [property: JsonPropertyName("countryFilter")] string? CountryFilter = null,
    [property: JsonPropertyName("searchQuery")] string? SearchQuery = null,
    [property: JsonPropertyName("minPopulation")] int? MinPopulation = null,
    [property: JsonPropertyName("maxPopulation")] int? MaxPopulation = null
) : PaginationRequest(Page, PageSize, SortBy, SortDirection);

// Bulk operation request with pagination
public record BulkOperationRequest<T>(
    [property: JsonPropertyName("items")] IEnumerable<T> Items,
    [property: JsonPropertyName("batchSize")] int BatchSize = 10,
    [property: JsonPropertyName("includePagination")] bool IncludePagination = true
);

// Bulk operation response with pagination
public record BulkOperationResponse<T>(
    [property: JsonPropertyName("results")] IEnumerable<T> Results,
    [property: JsonPropertyName("successCount")] int SuccessCount,
    [property: JsonPropertyName("failureCount")] int FailureCount,
    [property: JsonPropertyName("batchInfo")] BatchInfo? BatchInfo
);

public record BatchInfo(
    [property: JsonPropertyName("batchSize")] int BatchSize,
    [property: JsonPropertyName("totalBatches")] int TotalBatches,
    [property: JsonPropertyName("currentBatch")] int CurrentBatch
);

// Helper class for creating paginated responses
public static class PaginationHelper
{
    public static PaginatedResponse<T> CreateResponse<T>(
        IEnumerable<T> data,
        int totalItems,
        PaginationRequest request)
    {
        var validatedPage = request.ValidatedPage;
        var validatedPageSize = request.ValidatedPageSize;
        var totalPages = (int)Math.Ceiling((double)totalItems / validatedPageSize);
        
        var firstItemIndex = totalItems == 0 ? 0 : (validatedPage - 1) * validatedPageSize + 1;
        var lastItemIndex = Math.Min(validatedPage * validatedPageSize, totalItems);

        var metadata = new PaginationMetadata(
            CurrentPage: validatedPage,
            PageSize: validatedPageSize,
            TotalItems: totalItems,
            TotalPages: totalPages,
            HasNextPage: validatedPage < totalPages,
            HasPreviousPage: validatedPage > 1,
            FirstItemIndex: firstItemIndex,
            LastItemIndex: lastItemIndex
        );

        return new PaginatedResponse<T>(data, metadata);
    }

    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, PaginationRequest request)
    {
        return query.Skip(request.Skip).Take(request.ValidatedPageSize);
    }

    public static IEnumerable<T> ApplyPagination<T>(this IEnumerable<T> items, PaginationRequest request)
    {
        return items.Skip(request.Skip).Take(request.ValidatedPageSize);
    }
} 