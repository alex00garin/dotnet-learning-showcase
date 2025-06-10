using DotnetLearningShowcase.Models;

namespace DotnetLearningShowcase.Services;

public interface IAutocompleteService
{
    Task<AutocompleteResponse> SearchAsync(AutocompleteRequest request);
    Task<AutocompleteResponse> SearchCitiesAsync(string query, int limit = 10);
    Task<List<string>> GetAvailableDataSourcesAsync();
    Task<bool> IsDataSourceAvailableAsync(string dataSourceName);
} 