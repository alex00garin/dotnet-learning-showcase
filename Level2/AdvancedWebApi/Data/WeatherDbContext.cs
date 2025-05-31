using Microsoft.EntityFrameworkCore;
using AdvancedWebApi.Models;

namespace AdvancedWebApi.Data;

public class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options)
        : base(options)
    {
    }

    public DbSet<WeatherRecord> WeatherRecords { get; set; } = null!;
    public DbSet<ForecastLog> ForecastLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherRecord>()
            .HasIndex(w => new { w.Location, w.Timestamp });

        modelBuilder.Entity<ForecastLog>()
            .HasIndex(f => new { f.City, f.RequestedAt });

        // Seed some initial data
        modelBuilder.Entity<WeatherRecord>().HasData(
            new WeatherRecord
            {
                Id = 1,
                Location = "London",
                Timestamp = new DateTime(2024, 3, 15, 10, 0, 0, DateTimeKind.Utc),
                Temperature = 18.5,
                Humidity = 65,
                WindSpeed = 12.3,
                WindDirection = "NW",
                Pressure = 1013.2,
                Conditions = "Partly Cloudy"
            },
            new WeatherRecord
            {
                Id = 2,
                Location = "London",
                Timestamp = new DateTime(2024, 3, 15, 11, 0, 0, DateTimeKind.Utc),
                Temperature = 19.2,
                Humidity = 62,
                WindSpeed = 11.8,
                WindDirection = "NW",
                Pressure = 1012.8,
                Conditions = "Sunny"
            }
        );

        // Seed forecast logs
        modelBuilder.Entity<ForecastLog>().HasData(
            new ForecastLog
            {
                Id = 1,
                City = "London",
                RequestedAt = new DateTime(2024, 3, 15, 9, 0, 0, DateTimeKind.Utc),
                Summary = "Mild with occasional clouds",
                TemperatureC = 18.5f
            },
            new ForecastLog
            {
                Id = 2,
                City = "London",
                RequestedAt = new DateTime(2024, 3, 15, 10, 0, 0, DateTimeKind.Utc),
                Summary = "Sunny with light breeze",
                TemperatureC = 19.2f
            }
        );
    }
} 