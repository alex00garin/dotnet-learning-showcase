using DotnetLearningShowcase.Models;

namespace DotnetLearningShowcase.Services;

public class AutocompleteService : IAutocompleteService
{
    private readonly Dictionary<string, IAutocompleteDataSource<object>> _dataSources;
    private readonly ILogger<AutocompleteService> _logger;

    public AutocompleteService(
        IEnumerable<IAutocompleteDataSource<object>> dataSources,
        ILogger<AutocompleteService> logger)
    {
        _dataSources = dataSources.ToDictionary(ds => ds.Name, ds => ds);
        _logger = logger;
    }

    public async Task<AutocompleteResponse> SearchAsync(AutocompleteRequest request)
    {
        if (!_dataSources.TryGetValue(request.DataSource, out var dataSource))
        {
            _logger.LogWarning("Data source '{DataSource}' not found", request.DataSource);
            return new AutocompleteResponse(
                Query: request.Query,
                Results: new List<AutocompleteItem>(),
                TotalCount: 0,
                DataSource: request.DataSource
            );
        }

        try
        {
            var results = await dataSource.SearchAsync(request.Query, request.Limit);
            
            return new AutocompleteResponse(
                Query: request.Query,
                Results: results,
                TotalCount: results.Count,
                DataSource: request.DataSource
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching in data source '{DataSource}' with query '{Query}'", 
                request.DataSource, request.Query);
            
            return new AutocompleteResponse(
                Query: request.Query,
                Results: new List<AutocompleteItem>(),
                TotalCount: 0,
                DataSource: request.DataSource
            );
        }
    }

    public async Task<AutocompleteResponse> SearchCitiesAsync(string query, int limit = 10)
    {
        return await SearchAsync(new AutocompleteRequest(query, limit, "cities"));
    }

    public Task<List<string>> GetAvailableDataSourcesAsync()
    {
        return Task.FromResult(_dataSources.Keys.ToList());
    }

    public Task<bool> IsDataSourceAvailableAsync(string dataSourceName)
    {
        return Task.FromResult(_dataSources.ContainsKey(dataSourceName));
    }
} 