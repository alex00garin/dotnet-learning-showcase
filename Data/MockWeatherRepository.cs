using Dapper;
using DotnetLearningShowcase.Models;
using Npgsql;

namespace DotnetLearningShowcase.Data;

public interface IMockWeatherRepository
{
    Task<IEnumerable<WeatherForecastRecord>> GetAllMockWeatherDataAsync();
    Task InitializeMockWeatherDataAsync();
}

public class MockWeatherRepository : IMockWeatherRepository
{
    private readonly string _connectionString;

    public MockWeatherRepository(IConfiguration configuration)
    {
        _connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? 
                          configuration.GetConnectionString("SupabaseDb") ?? 
                          throw new InvalidOperationException("Database connection string not found.");
    }

    public async Task InitializeMockWeatherDataAsync()
    {
        try
        {
            using var db = new NpgsqlConnection(_connectionString);
            await db.OpenAsync();

            // Ensure the mock_weather_data table exists (Level 4 table)
            var createTableSql = @"
                CREATE TABLE IF NOT EXISTS public.mock_weather_data (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    city TEXT NOT NULL,
                    country TEXT NOT NULL,
                    date DATE NOT NULL,
                    temperature_c INTEGER NOT NULL,
                    summary TEXT
                );
            ";
            await db.ExecuteAsync(createTableSql);

            // Check if we have any data
            var countSql = "SELECT COUNT(*) FROM public.mock_weather_data";
            var count = await db.QuerySingleAsync<int>(countSql);
            
            if (count == 0)
            {
                // Seed with sample data for Level 4 testing
                await SeedMockWeatherDataAsync(db);
                Console.WriteLine("Mock weather data seeded successfully.");
            }
            else
            {
                Console.WriteLine($"Mock weather data table initialized with {count} records.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing mock weather data: {ex.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<WeatherForecastRecord>> GetAllMockWeatherDataAsync()
    {
        var sql = @"
            SELECT 
                id,
                city,
                country,
                date,
                temperature_c AS TemperatureC,
                summary
            FROM public.mock_weather_data 
            ORDER BY date DESC, city
        ";

        using var db = new NpgsqlConnection(_connectionString);
        return await db.QueryAsync<WeatherForecastRecord>(sql);
    }

    private async Task SeedMockWeatherDataAsync(NpgsqlConnection db)
    {
        var seedData = GenerateMockWeatherData();
        
        var sql = @"
            INSERT INTO public.mock_weather_data (id, city, country, date, temperature_c, summary)
            VALUES (@Id, @City, @Country, @Date, @TemperatureC, @Summary)
        ";

        foreach (var record in seedData)
        {
            await db.ExecuteAsync(sql, record);
        }
    }

    private List<object> GenerateMockWeatherData()
    {
        var cities = new[]
        {
            ("London", "UK"),
            ("Paris", "France"),
            ("Berlin", "Germany"),
            ("Madrid", "Spain"),
            ("Rome", "Italy"),
            ("Amsterdam", "Netherlands"),
            ("Stockholm", "Sweden"),
            ("Vienna", "Austria"),
            ("Prague", "Czech Republic"),
            ("Warsaw", "Poland"),
            ("Budapest", "Hungary"),
            ("Copenhagen", "Denmark"),
            ("Oslo", "Norway"),
            ("Helsinki", "Finland"),
            ("Dublin", "Ireland"),
            ("Lisbon", "Portugal"),
            ("Brussels", "Belgium"),
            ("Zurich", "Switzerland"),
            ("Athens", "Greece"),
            ("Sofia", "Bulgaria")
        };

        var weatherTypes = new[]
        {
            "Sunny", "Cloudy", "Rainy", "Snowy", "Windy", "Foggy", "Stormy", "Partly Cloudy", "Overcast", "Drizzle"
        };

        var random = new Random(42); // Fixed seed for consistent data
        var records = new List<object>();
        var startDate = DateTime.Today.AddDays(-60);

        // Generate 100 records (5 days of weather for each city)
        foreach (var (city, country) in cities)
        {
            for (int day = 0; day < 5; day++)
            {
                var date = startDate.AddDays(day);
                var temperature = random.Next(-5, 35); // -5°C to 35°C
                var weatherType = weatherTypes[random.Next(weatherTypes.Length)];
                var summary = $"{weatherType} in {city}, {country}";

                records.Add(new
                {
                    Id = Guid.NewGuid(),
                    City = city,
                    Country = country,
                    Date = date,
                    TemperatureC = temperature,
                    Summary = summary
                });
            }
        }

        return records;
    }
} 