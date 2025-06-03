using System.Text.Json.Serialization;
using DotnetLearningShowcase.Data;
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

        // Register services
        services.AddScoped<IWeatherService, WeatherService>();
        services.AddScoped<IWeatherRepository, WeatherRepository>();

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

        // Configure CORS
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
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

        app.UseCors();
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