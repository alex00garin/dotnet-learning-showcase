using System;

namespace AdvancedWebApi.Models;

public class ForecastLog
{
    public int Id { get; set; }
    public string City { get; set; } = default!;
    public DateTime RequestedAt { get; set; }
    public string Summary { get; set; } = default!;
    public float TemperatureC { get; set; }
    public float TemperatureF => 32 + (int)(TemperatureC / 0.5556);
} 