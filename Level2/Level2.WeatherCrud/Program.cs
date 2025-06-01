using Npgsql;
using Dapper;
using System.Text.Json.Serialization;
using System.Text.Json;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });

        // Configure JSON serialization
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        var connectionString = builder.Configuration.GetConnectionString("SupabaseDb");
        builder.Services.AddScoped<NpgsqlConnection>(_ => new NpgsqlConnection(connectionString));

        var app = builder.Build();

        // One-time data insertion (Development only)
        async Task InsertSampleData()
        {
            using var db = new NpgsqlConnection(connectionString);
            await db.OpenAsync();
            
            var sql = """
                INSERT INTO public.weather_forecasts (id, city, country, date, temperature_c, summary) 
                VALUES 
                    ('5fd6d4de-e0c7-4c21-9667-d555bd533aa8', 'London', 'UK', '2024-03-20', 20, 'Sunny'),
                    ('903773be-4dc0-428d-a275-4ef9ecc6272a', 'London', 'UK', '2024-03-20', 20, 'Sunny')
                ON CONFLICT (id) DO NOTHING;
            """;
            
            await db.ExecuteAsync(sql);
        }

        // Run the data insertion only in Development environment
        if (app.Environment.IsDevelopment())
        {
            _ = InsertSampleData();
        }

        // Enable CORS
        app.UseCors();

        // Health check endpoint
        app.MapGet("/health", () => Results.Text("Healthy"));

        app.MapGet("/", () => "Hello World!");

        app.MapPost("/weatherforecast/save", async (NpgsqlConnection db, WeatherSaveRequest request) =>
        {
            var sql = """
                insert into weather_forecasts (city, country, date, temperature_c, summary)
                values (@City, @Country, @Date, @TemperatureC, @Summary)
                """;

            foreach (var f in request.Forecasts)
            {
                await db.ExecuteAsync(sql, new
                {
                    request.City,
                    request.Country,
                    Date = f.Date.ToDateTime(TimeOnly.MinValue),
                    f.TemperatureC,
                    f.Summary
                });
            }

            return Results.Text("Saved");
        });

        app.MapGet("/weatherforecast/history/{city}", async (NpgsqlConnection db, string city) =>
        {
            var sql = """
                select 
                    id,
                    city,
                    country,
                    date,
                    temperature_c as temperaturec,
                    summary
                from weather_forecasts 
                where city = @City 
                order by date
            """;
            var results = await db.QueryAsync<WeatherForecastRecord>(sql, new { City = city });
            var json = JsonSerializer.Serialize(results);
            return Results.Text(json, "application/json");
        });

        app.MapPut("/weatherforecast/update/{id}", async (NpgsqlConnection db, Guid id, UpdateSummaryRequest request) =>
        {
            var sql = "update weather_forecasts set summary = @Summary where id = @Id";
            var affected = await db.ExecuteAsync(sql, new { Id = id, Summary = request.Summary });
            return affected > 0 
                ? Results.Text("Updated") 
                : Results.Text("Not found", statusCode: 404);
        });

        app.MapDelete("/weatherforecast/delete/{id}", async (NpgsqlConnection db, Guid id) =>
        {
            var sql = "delete from weather_forecasts where id = @Id";
            var affected = await db.ExecuteAsync(sql, new { Id = id });
            return affected > 0 
                ? Results.Text("Deleted") 
                : Results.Text("Not found", statusCode: 404);
        });

        app.Run();
    }
}

record ForecastDto(DateOnly Date, int TemperatureC, string Summary);
record WeatherSaveRequest(string City, string Country, List<ForecastDto> Forecasts);
record WeatherForecastRecord(
    Guid id,
    string city,
    string country,
    DateTime date,
    int temperaturec,
    string summary
);
record UpdateSummaryRequest(string Summary);

