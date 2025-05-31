using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdvancedWebApi.Data;
using AdvancedWebApi.Models;

namespace AdvancedWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly WeatherDbContext _context;

    public WeatherController(WeatherDbContext context)
    {
        _context = context;
    }

    [HttpGet("{location}")]
    public async Task<ActionResult<IEnumerable<WeatherRecord>>> GetWeatherHistory(string location)
    {
        var records = await _context.WeatherRecords
            .Where(w => w.Location == location)
            .OrderByDescending(w => w.Timestamp)
            .Take(24) // Last 24 records
            .ToListAsync();

        if (!records.Any())
        {
            return NotFound($"No weather records found for {location}");
        }

        return records;
    }

    [HttpGet("{location}/current")]
    public async Task<ActionResult<WeatherRecord>> GetCurrentWeather(string location)
    {
        var record = await _context.WeatherRecords
            .Where(w => w.Location == location)
            .OrderByDescending(w => w.Timestamp)
            .FirstOrDefaultAsync();

        if (record == null)
        {
            return NotFound($"No weather records found for {location}");
        }

        return record;
    }

    [HttpPost]
    public async Task<ActionResult<WeatherRecord>> AddWeatherRecord(WeatherRecord record)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(record.Location))
        {
            return BadRequest("Location is required.");
        }
        record.Timestamp = DateTime.UtcNow;
        _context.WeatherRecords.Add(record);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCurrentWeather), new { location = record.Location }, record);
    }
} 