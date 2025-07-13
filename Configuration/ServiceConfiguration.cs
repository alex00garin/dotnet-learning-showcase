using System.Text.Json.Serialization;
using DotnetLearningShowcase.Data;
using DotnetLearningShowcase.Models;
using DotnetLearningShowcase.Services;
using Microsoft.AspNetCore.Http.Json;

namespace DotnetLearningShowcase.Configuration;

public static class ServiceConfiguration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register HTTP clients
        services.AddHttpClient<IGeocodingService, GeocodingService>();
        services.AddHttpClient<IWeatherApiService, WeatherApiService>();
        services.AddHttpClient(); // For autocomplete data sources

        // Register services
        services.AddScoped<IWeatherService, WeatherService>();
        services.AddScoped<IWeatherRepository, WeatherRepository>();
        
        // Register autocomplete services
        services.AddScoped<IAutocompleteService, AutocompleteService>();
        services.AddScoped<IAutocompleteDataSource<object>, CitiesDataSource>();
        services.AddScoped<IAutocompleteDataSource<object>, CountriesDataSource>();
        
        // Register pagination service
        services.AddScoped<IPaginationService, PaginationService>();

        return services;
    }

    public static IServiceCollection AddApiConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Dotnet Learning Showcase API",
                Version = "v1",
                Description = "A sample API demonstrating weather forecast functionality with two levels of complexity"
            });
        });

        // Configure CORS with specific origins and better error handling
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(
                        "https://www.alexandergarin.com",
                        "https://alexandergarin.com",
                        "http://localhost:3000",
                        "http://localhost:5173",
                        "http://localhost:8080"
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition");
            });
            
            // Fallback policy for development
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .WithExposedHeaders("Content-Disposition");
            });
        });

        // JSON configuration for HTTP endpoints
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

        // JSON configuration for general serialization
        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

        return services;
    }

    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        // Development-specific middleware
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dotnet Learning Showcase API v1");
                c.RoutePrefix = "swagger";
            });
        }

        // Configure CORS before other middleware to ensure headers are set on all responses
        if (app.Environment.IsDevelopment())
        {
            app.UseCors("AllowAll");
        }
        else
        {
            app.UseCors(); // Use default policy
        }

        // Add global error handling middleware to ensure CORS headers on error responses
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                // Log the error
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Unhandled exception occurred");

                // Ensure CORS headers are present on error responses
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";
                    
                    var errorResponse = new
                    {
                        error = "Internal server error",
                        message = app.Environment.IsDevelopment() ? ex.Message : "An error occurred processing your request",
                        timestamp = DateTime.UtcNow
                    };
                    
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(errorResponse));
                }
            }
        });

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        return app;
    }

    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IWeatherRepository>();
            
            try
            {
                await repository.InitializeDatabaseAsync();
                Console.WriteLine("Database initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization failed: {ex.Message}");
            }
        }
    }
} 