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

            // Ensure the weather_forecasts table exists (Level 2 table)
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

            // No hardcoded seed data - Level 2 uses weather_forecasts table
            Console.WriteLine("Database initialized successfully.");
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
            INSERT INTO public.weather_forecasts (id, city, country, date, temperature_c, summary)
            VALUES (@Id, @City, @Country, @Date, @TemperatureC, @Summary)
        ";

        using var db = new NpgsqlConnection(_connectionString);
        await db.OpenAsync();

        foreach (var forecast in forecasts)
        {
            await db.ExecuteAsync(sql, new
            {
                Id = Guid.NewGuid(),
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
            FROM public.weather_forecasts 
            WHERE LOWER(city) = LOWER(@City)
            ORDER BY date
        ";

        using var db = new NpgsqlConnection(_connectionString);
        return await db.QueryAsync<WeatherForecastRecord>(sql, new { City = city });
    }

    public async Task<bool> UpdateSummaryAsync(Guid id, string summary)
    {
        var sql = "UPDATE public.weather_forecasts SET summary = @Summary WHERE id = @Id";
        
        using var db = new NpgsqlConnection(_connectionString);
        var affected = await db.ExecuteAsync(sql, new { Id = id, Summary = summary });
        return affected > 0;
    }

    public async Task<bool> DeleteForecastAsync(Guid id)
    {
        var sql = "DELETE FROM public.weather_forecasts WHERE id = @Id";
        
        using var db = new NpgsqlConnection(_connectionString);
        var affected = await db.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<IEnumerable<WeatherForecastRecord>> GetAllRecordsAsync()
    {
        var sql = @"
            SELECT 
                id,
                city,
                country,
                date,
                temperature_c AS TemperatureC,
                summary
            FROM public.weather_forecasts 
            ORDER BY date DESC, city
        ";

        using var db = new NpgsqlConnection(_connectionString);
        return await db.QueryAsync<WeatherForecastRecord>(sql);
    }
} 