using DotnetLearningShowcase.Models;

namespace DotnetLearningShowcase.Services;

// Example of another data source to demonstrate reusability
public class CountriesDataSource : IAutocompleteDataSource<object>
{
    private readonly ILogger<CountriesDataSource> _logger;
    private static readonly List<CountryData> _countries = new()
    {
        new("AF", "Afghanistan"),
        new("AL", "Albania"),
        new("DZ", "Algeria"),
        new("AD", "Andorra"),
        new("AO", "Angola"),
        new("AR", "Argentina"),
        new("AM", "Armenia"),
        new("AU", "Australia"),
        new("AT", "Austria"),
        new("AZ", "Azerbaijan"),
        new("BD", "Bangladesh"),
        new("BE", "Belgium"),
        new("BR", "Brazil"),
        new("BG", "Bulgaria"),
        new("CA", "Canada"),
        new("CN", "China"),
        new("CO", "Colombia"),
        new("CR", "Costa Rica"),
        new("HR", "Croatia"),
        new("CU", "Cuba"),
        new("DK", "Denmark"),
        new("EG", "Egypt"),
        new("FI", "Finland"),
        new("FR", "France"),
        new("DE", "Germany"),
        new("GR", "Greece"),
        new("IN", "India"),
        new("ID", "Indonesia"),
        new("IR", "Iran"),
        new("IQ", "Iraq"),
        new("IE", "Ireland"),
        new("IT", "Italy"),
        new("JP", "Japan"),
        new("KZ", "Kazakhstan"),
        new("KE", "Kenya"),
        new("MX", "Mexico"),
        new("NL", "Netherlands"),
        new("NZ", "New Zealand"),
        new("NO", "Norway"),
        new("PK", "Pakistan"),
        new("PE", "Peru"),
        new("PL", "Poland"),
        new("PT", "Portugal"),
        new("RO", "Romania"),
        new("RU", "Russia"),
        new("SA", "Saudi Arabia"),
        new("ZA", "South Africa"),
        new("KR", "South Korea"),
        new("ES", "Spain"),
        new("SE", "Sweden"),
        new("CH", "Switzerland"),
        new("TH", "Thailand"),
        new("TR", "Turkey"),
        new("UA", "Ukraine"),
        new("AE", "United Arab Emirates"),
        new("GB", "United Kingdom"),
        new("US", "United States"),
        new("VN", "Vietnam")
    };

    public string Name => "countries";

    public CountriesDataSource(ILogger<CountriesDataSource> logger)
    {
        _logger = logger;
    }

    public Task<List<object>> GetAllDataAsync()
    {
        return Task.FromResult(_countries.Cast<object>().ToList());
    }

    public Task<List<AutocompleteItem>> SearchAsync(string query, int limit)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult(new List<AutocompleteItem>());
        }

        var normalizedQuery = query.Trim().ToLowerInvariant();

        var results = _countries
            .Where(country => country.Name.ToLowerInvariant().Contains(normalizedQuery) ||
                             country.Code.ToLowerInvariant().Contains(normalizedQuery))
            .OrderBy(country => country.Name.ToLowerInvariant().IndexOf(normalizedQuery))
            .ThenBy(country => country.Name.Length)
            .Take(limit)
            .Select(country => new AutocompleteItem(
                Id: country.Code,
                Label: $"{country.Name} ({country.Code})",
                Value: country.Name,
                Metadata: new Dictionary<string, object>
                {
                    ["code"] = country.Code,
                    ["type"] = "country"
                }
            ))
            .ToList();

        return Task.FromResult(results);
    }
}

public record CountryData(string Code, string Name); 