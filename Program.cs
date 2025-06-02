using Npgsql;
using Dapper;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Net.Http;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for different environments
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Use default ports for development and 8080 for production (Fly.io)
    if (builder.Environment.IsDevelopment())
    {
        // Default ports from Properties/launchSettings.json will be used
    }
    else
    {
        serverOptions.ListenAnyIP(8080); // Fly.io default port
    }
});

// Common services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Content-Disposition");
    });
});

// JSON configuration
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Database connection (for Level2)
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? 
                       builder.Configuration.GetConnectionString("SupabaseDb");
builder.Services.AddScoped<NpgsqlConnection>(_ => new NpgsqlConnection(connectionString));

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseStaticFiles();

// Root endpoint
app.MapGet("/", () => "Dotnet Learning Showcase - Choose /level1 or /level2 routes");

// Health check endpoint
app.MapGet("/health", () => Results.Ok("Healthy"))
   .WithName("HealthCheck");

// -------------------------------
// Level 1 - Basic Weather API
// -------------------------------
var level1 = app.MapGroup("/level1");

level1.MapGet("/", () => "Level 1 - Basic Weather API");

level1.MapGet("/weatherforecast", async (IHttpClientFactory httpClientFactory, string? city = "Berlin") =>
{
    var httpClient = httpClientFactory.CreateClient();

    var geocodingUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(city ?? "Berlin")}&count=1";
    var geocodingResponse = await httpClient.GetStringAsync(geocodingUrl);

    using var geocodingDoc = JsonDocument.Parse(geocodingResponse);
    var results = geocodingDoc.RootElement.GetProperty("results");

    if (results.GetArrayLength() == 0)
    {
        return Results.NotFound($"City '{city}' not found.");
    }

    var location = results[0];
    var latitude = location.GetProperty("latitude").GetDouble();
    var longitude = location.GetProperty("longitude").GetDouble();
    var cityName = location.GetProperty("name").GetString() ?? city;
    var country = location.GetProperty("country").GetString() ?? "Unknown";

    var weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&daily=temperature_2m_max,temperature_2m_min,precipitation_probability_max&timezone=auto";
    var weatherResponse = await httpClient.GetStringAsync(weatherUrl);

    using var weatherDoc = JsonDocument.Parse(weatherResponse);
    var daily = weatherDoc.RootElement.GetProperty("daily");

    var dates = daily.GetProperty("time").EnumerateArray();
    var temps = daily.GetProperty("temperature_2m_max").EnumerateArray();
    var precip = daily.GetProperty("precipitation_probability_max").EnumerateArray();

    var forecasts = dates.Zip(
        temps.Zip(precip, (temp, prob) => (temp.GetDouble(), prob.GetDouble())),
        (date, data) =>
        {
            var (temp, prob) = data;
            var summary = prob > 50 ? "Rainy" :
                          temp > 25 ? "Hot" :
                          temp > 20 ? "Warm" :
                          temp > 15 ? "Mild" :
                          temp > 10 ? "Cool" :
                          temp > 5 ? "Chilly" : "Cold";

            return new WeatherForecast(
                DateOnly.Parse(date.GetString() ?? DateTime.UtcNow.ToString("yyyy-MM-dd")),
                (int)temp,
                $"{summary} in {cityName}, {country}"
            );
        }).ToArray();

    return Results.Ok(new
    {
        Location = $"{cityName}, {country}",
        Forecasts = forecasts
    });
})
.WithName("Level1_GetWeatherForecast");

// -------------------------------
// Level 2 - Weather CRUD
// -------------------------------
var level2 = app.MapGroup("/level2");

level2.MapGet("/", () => "Level 2 - Weather CRUD");

// Database initialization for Level2 (in Development only)
if (app.Environment.IsDevelopment() && !string.IsNullOrEmpty(connectionString))
{
    // Use Task.Run in fire-and-forget mode
    _ = Task.Run(async () =>
    {
        try
        {
            using var db = new NpgsqlConnection(connectionString);
            await db.OpenAsync();

            // Create table if it doesn't exist
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

            // Insert sample data
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
        }
    });
}

level2.MapPost("/weatherforecast/save", async (NpgsqlConnection db, WeatherSaveRequest request) =>
{
    var sql = @"
        INSERT INTO weather_forecasts (city, country, date, temperature_c, summary)
        VALUES (@City, @Country, @Date, @TemperatureC, @Summary)
    ";

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

level2.MapGet("/weatherforecast/history/{city}", async (NpgsqlConnection db, string city) =>
{
    var sql = @"
        SELECT 
            id,
            city,
            country,
            date,
            temperature_c AS temperaturec,
            summary
        FROM weather_forecasts 
        WHERE city = @City 
        ORDER BY date
    ";
    var results = await db.QueryAsync<WeatherForecastRecord>(sql, new { City = city });
    var json = JsonSerializer.Serialize(results);
    return Results.Text(json, "application/json");
});

level2.MapPut("/weatherforecast/update/{id}", async (NpgsqlConnection db, Guid id, UpdateSummaryRequest request) =>
{
    var sql = "UPDATE weather_forecasts SET summary = @Summary WHERE id = @Id";
    var affected = await db.ExecuteAsync(sql, new { Id = id, Summary = request.Summary });
    return affected > 0 
        ? Results.Text("Updated") 
        : Results.Text("Not found", statusCode: 404);
});

level2.MapDelete("/weatherforecast/delete/{id}", async (NpgsqlConnection db, Guid id) =>
{
    var sql = "DELETE FROM weather_forecasts WHERE id = @Id";
    var affected = await db.ExecuteAsync(sql, new { Id = id });
    return affected > 0 
        ? Results.Text("Deleted") 
        : Results.Text("Not found", statusCode: 404);
});

// Start the application
Console.WriteLine("\nAvailable Endpoints:");
Console.WriteLine("GET /level1/weatherforecast?city={cityName}");
Console.WriteLine("GET /level2/weatherforecast/history/{city}");
Console.WriteLine("POST /level2/weatherforecast/save");
Console.WriteLine("PUT /level2/weatherforecast/update/{id}");
Console.WriteLine("DELETE /level2/weatherforecast/delete/{id}");
Console.WriteLine("GET /health");

app.Run();

// Common record types
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Level2 record types
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
