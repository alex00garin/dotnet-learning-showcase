using Microsoft.OpenApi.Models;
using System.Net.Http;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Services
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
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();

// Endpoint
app.MapGet("/weatherforecast", async (IHttpClientFactory httpClientFactory, string? city = "Berlin") =>
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
.WithName("GetWeatherForecast");

// Health check endpoint
app.MapGet("/health", () => Results.Ok("Healthy"))
   .WithName("HealthCheck");

// Console info
Console.WriteLine("\nAvailable Endpoints:");
Console.WriteLine("GET /weatherforecast?city={cityName}");
Console.WriteLine("GET /swagger");
Console.WriteLine("GET /swagger/v1/swagger.json");
Console.WriteLine("GET /health");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
