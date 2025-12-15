using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WSV.Api.Data;
using WSV.Api.Services;

namespace WSV.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReadingsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILastReadingService _lastReadingService;

    public ReadingsController(AppDbContext context, ILastReadingService lastReadingService)
    {
        _context = context;
        _lastReadingService = lastReadingService;
    }   

    //GET only the latest reading for all the sources /api/readings/latest
    //First attemp cycled in EF Core and JSON file. Use of DTO fixed it.
    //Pure cache service, no queries
    [HttpGet("latest")]
    public IActionResult GetLatestAll()
    {
       var latest = _lastReadingService.GetAll();

       var dtoList = latest.Select(r => new ReadingDto
       {
            SourceId = r.SourceId,
            Timestamp = r.Timestamp,
            Status = r.Status,
            RPM = r.RPM,
            Power = r.Power,
            Temperature = r.Temperature
       }).ToList();

       return Ok(dtoList);
    }

    //GET history of single source from Time to Time /api/readings/source/{SourceId}?...
    //From and To will be added as query information
    //Pure DB query, DATA-LAKE like
    [HttpGet("source/{sourceId}")]
    public async Task <IActionResult> GetHistoryOne(
        int sourceId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? limit
        )
    {
        var query = _context.SourceReadings
            .AsNoTracking()
            .Where(t => t.SourceId == sourceId);

        if(from.HasValue)
        {
            query = query.Where(u => u.Timestamp >= from.Value);    
        }

        if(to.HasValue)
        {
            query = query.Where(v => v.Timestamp < to.Value);
        }

        // Hard cap of 5000 readings to prevent accidental overloading
        // Basic soft cap stays at 1000
        var take = Math.Clamp(limit ?? 1000, 1, 5000);
        
        var readings = await query 
            .OrderByDescending(r => r.Timestamp)  
            .Take(take)
            // This is where whole query is executed using ToListAsync, FirstAsync etc.
            // Everthing up to this points is just preparation for actual SDF exectuion here.   
            .ToListAsync();
        
        var dto = readings.Select(x => new ReadingDto
        {
            SourceId = x.SourceId,
            Timestamp = x.Timestamp,
            Status = x.Status,
            RPM = x.RPM,
            Power = x.Power,
            Temperature = x.Temperature
        }).ToList();

        return Ok(dto);
    }

    //GET all readings endpoint /api/readings
    //Simulates access to the DL and provides whole history
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var readings = await _context.SourceReadings
        .OrderBy(r => r.SourceId)
        .ThenByDescending(r => r.Timestamp)
        .ToListAsync();

        return Ok(readings);
    }
}