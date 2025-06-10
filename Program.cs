using DotnetLearningShowcase.Configuration;
using DotnetLearningShowcase.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for different environments
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Use port 8080 in production (Fly.io default)
    if (!builder.Environment.IsDevelopment())
    {
        serverOptions.ListenAnyIP(8080);
    }
    // Development uses default ports from Properties/launchSettings.json
});

// Add services using extension methods
builder.Services.AddApplicationServices();
builder.Services.AddApiConfiguration();

var app = builder.Build();

// Configure middleware
app.ConfigureMiddleware();

// Initialize database in development
await app.InitializeDatabaseAsync();

// Root endpoint
app.MapGet("/", () => "Dotnet Learning Showcase - Choose /level1 or /level2 routes");

// Health check endpoint
app.MapGet("/health", () => Results.Ok("Healthy"))
   .WithName("HealthCheck");

// Map endpoint groups
app.MapLevel1Endpoints();
app.MapLevel2Endpoints();
app.MapLevel3Endpoints();

// Start the application
Console.WriteLine("\nAvailable Endpoints:");
Console.WriteLine("=== Level 1 - Weather API ===");
Console.WriteLine("GET /level1/weatherforecast?city={cityName}");
Console.WriteLine("GET /level1/weatherforecast/smart?city={cityName} (with autocomplete)");
Console.WriteLine("");
Console.WriteLine("=== Level 2 - Database CRUD ===");
Console.WriteLine("GET /level2/weatherforecast/history/{city}");
Console.WriteLine("GET /level2/weatherforecast/history/smart/{city} (with fuzzy matching)");
Console.WriteLine("POST /level2/weatherforecast/bulk (multiple cities with autocomplete)");
Console.WriteLine("POST /level2/weatherforecast/save");
Console.WriteLine("PUT /level2/weatherforecast/update/{id}");
Console.WriteLine("DELETE /level2/weatherforecast/delete/{id}");
Console.WriteLine("");
Console.WriteLine("=== Level 3 - Autocomplete Service ===");
Console.WriteLine("GET /level3/cities/autocomplete?query={partialCityName}");
Console.WriteLine("POST /level3/autocomplete");
Console.WriteLine("GET /level3/datasources");
Console.WriteLine("");
Console.WriteLine("GET /health");

app.Run();

// Make Program class accessible for testing
public partial class Program { }
