using Dapper;
using DotnetLearningShowcase.Models;
using Npgsql;

namespace DotnetLearningShowcase.Data;

public class WeatherRepository : IWeatherRepository
{
    private readonly string _connectionString;

    public WeatherRepository(IConfiguration configuration)
    {
        _connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? 
                          configuration.GetConnectionString("SupabaseDb") ?? 
                          throw new InvalidOperationException("Database connection string not found.");
    }

    public async Task InitializeDatabaseAsync()
    {
        try
        {
            using var db = new NpgsqlConnection(_connectionString);
            await db.OpenAsync();

            var createTableSql = @"
                CREATE TABLE IF NOT EXISTS public.weather_forecasts (
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    city TEXT NOT NULL,
                    country TEXT NOT NULL,
                    date DATE NOT NULL,
                    temperature_c INTEGER NOT NULL,
                    summary TEXT
                );
            ";
            await db.ExecuteAsync(createTableSql);

            var insertSampleSql = @"
                INSERT INTO public.weather_forecasts (id, city, country, date, temperature_c, summary) 
                VALUES 
                    ('5fd6d4de-e0c7-4c21-9667-d555bd533aa8', 'London', 'UK', '2024-03-20', 20, 'Sunny'),
                    ('903773be-4dc0-428d-a275-4ef9ecc6272a', 'London', 'UK', '2024-03-21', 18, 'Partly Cloudy')
                ON CONFLICT (id) DO NOTHING;
            ";
            await db.ExecuteAsync(insertSampleSql);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing database: {ex.Message}");
            throw;
        }
    }

    public async Task SaveForecastsAsync(string city, string country, IEnumerable<ForecastDto> forecasts)
    {
        var sql = @"
            INSERT INTO weather_forecasts (city, country, date, temperature_c, summary)
            VALUES (@City, @Country, @Date, @TemperatureC, @Summary)
        ";

        using var db = new NpgsqlConnection(_connectionString);
        await db.OpenAsync();

        foreach (var forecast in forecasts)
        {
            await db.ExecuteAsync(sql, new
            {
                City = city,
                Country = country,
                Date = forecast.Date.ToDateTime(TimeOnly.MinValue),
                TemperatureC = forecast.TemperatureC,
                Summary = forecast.Summary
            });
        }
    }

    public async Task<IEnumerable<WeatherForecastRecord>> GetHistoryAsync(string city)
    {
        var sql = @"
            SELECT 
                id,
                city,
                country,
                date,
                temperature_c AS TemperatureC,
                summary
            FROM weather_forecasts 
            WHERE city = @City 
            ORDER BY date
        ";

        using var db = new NpgsqlConnection(_connectionString);
        return await db.QueryAsync<WeatherForecastRecord>(sql, new { City = city });
    }

    public async Task<bool> UpdateSummaryAsync(Guid id, string summary)
    {
        var sql = "UPDATE weather_forecasts SET summary = @Summary WHERE id = @Id";
        
        using var db = new NpgsqlConnection(_connectionString);
        var affected = await db.ExecuteAsync(sql, new { Id = id, Summary = summary });
        return affected > 0;
    }

    public async Task<bool> DeleteForecastAsync(Guid id)
    {
        var sql = "DELETE FROM weather_forecasts WHERE id = @Id";
        
        using var db = new NpgsqlConnection(_connectionString);
        var affected = await db.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }
} 