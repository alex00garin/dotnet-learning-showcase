using DotnetLearningShowcase.Configuration;
using DotnetLearningShowcase.Endpoints;

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

// Start the application
Console.WriteLine("\nAvailable Endpoints:");
Console.WriteLine("GET /level1/weatherforecast?city={cityName}");
Console.WriteLine("GET /level2/weatherforecast/history/{city}");
Console.WriteLine("POST /level2/weatherforecast/save");
Console.WriteLine("PUT /level2/weatherforecast/update/{id}");
Console.WriteLine("DELETE /level2/weatherforecast/delete/{id}");
Console.WriteLine("GET /health");

app.Run();

// Make Program class accessible for testing
public partial class Program { }
