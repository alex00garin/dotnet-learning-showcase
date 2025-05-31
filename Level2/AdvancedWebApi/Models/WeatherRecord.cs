using System;

namespace AdvancedWebApi.Models;

public class WeatherRecord
{
    public int Id { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public double WindSpeed { get; set; }
    public string WindDirection { get; set; } = string.Empty;
    public double Pressure { get; set; }
    public string Conditions { get; set; } = string.Empty;
} 