using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdvancedWebApi.Data;
using AdvancedWebApi.Models;

namespace AdvancedWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ForecastLogsController : ControllerBase
{
    private readonly WeatherDbContext _context;

    public ForecastLogsController(WeatherDbContext context)
    {
        _context = context;
    }

    // GET: api/ForecastLogs
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ForecastLog>>> GetForecastLogs()
    {
        return await _context.ForecastLogs
            .OrderByDescending(f => f.RequestedAt)
            .ToListAsync();
    }

    // GET: api/ForecastLogs/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ForecastLog>> GetForecastLog(int id)
    {
        var forecastLog = await _context.ForecastLogs.FindAsync(id);

        if (forecastLog == null)
        {
            return NotFound();
        }

        return forecastLog;
    }

    // GET: api/ForecastLogs/city/London
    [HttpGet("city/{city}")]
    public async Task<ActionResult<IEnumerable<ForecastLog>>> GetForecastLogsByCity(string city)
    {
        var logs = await _context.ForecastLogs
            .Where(f => f.City == city)
            .OrderByDescending(f => f.RequestedAt)
            .ToListAsync();

        if (!logs.Any())
        {
            return NotFound($"No forecast logs found for {city}");
        }

        return logs;
    }

    // POST: api/ForecastLogs
    [HttpPost]
    public async Task<ActionResult<ForecastLog>> CreateForecastLog(ForecastLog forecastLog)
    {
        forecastLog.RequestedAt = DateTime.UtcNow;
        _context.ForecastLogs.Add(forecastLog);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetForecastLog), new { id = forecastLog.Id }, forecastLog);
    }

    // PUT: api/ForecastLogs/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateForecastLog(int id, ForecastLog forecastLog)
    {
        if (id != forecastLog.Id)
        {
            return BadRequest();
        }

        _context.Entry(forecastLog).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ForecastLogExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/ForecastLogs/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteForecastLog(int id)
    {
        var forecastLog = await _context.ForecastLogs.FindAsync(id);
        if (forecastLog == null)
        {
            return NotFound();
        }

        _context.ForecastLogs.Remove(forecastLog);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ForecastLogExists(int id)
    {
        return _context.ForecastLogs.Any(e => e.Id == id);
    }
} 