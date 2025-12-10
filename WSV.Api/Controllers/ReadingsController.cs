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

       var dtoList = latest.Select(r => new ReadingDTO
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

    //GET only the latest reading for one particular source /api/readings/source/{SourceId}/latest
    [HttpGet("source/{sourceId}/latest")]
    public async Task<IActionResult> GetLatestOne(int sourceId)
    {
        var source = await _context.Sources
            .FirstOrDefaultAsync(s => s.Id == sourceId);
        if(source == null)
        {
            return NotFound($"Source with ID {sourceId} wasnt found.");
        }

        var last = _lastReadingService.GetOne(sourceId);
        if(last == null)
        {
            return NotFound($"There are no readings yet for the source {sourceId}.");
        }

        var latestOne = new ReadingDTO
        {
            SourceId = sourceId,
            SourceName = source.Name,

            Timestamp = last.Timestamp,
            Status = last.Status,
            RPM = last.RPM,
            Power = last.Power,
            Temperature = last.Temperature
        };

        return Ok(latestOne);
    }

    //GET history of single source from Time to Time /api/readings/source/{SourceId}?...
    //From and To will be added as query information
    //Pure DB query, DATALIKE like
    [HttpGet("source/{sourceId}")]
    public async Task <IActionResult> GetHistoryOne(
        int sourceId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to
        )
    {
        var source = await _context.Sources
            .FirstOrDefaultAsync(s => s.Id == sourceId);
        if(source is null)
        {
            return NotFound($"Source with ID {sourceId} wasnt found.");
        }

        var query = _context.SourceReadings
            .Where(t => t.SourceId == sourceId);

        if(from.HasValue)
        {
            query = query.Where(u => u.Timestamp >= from.Value);    
        }

        if(to.HasValue)
        {
            query = query.Where(v => v.Timestamp < to.Value);
        }
            
        var readings = await query 
            .OrderBy(w => w.Timestamp)
            // This is where whole query is executed using ToListAsync, FirstAsync etc.
            // Everthing up to this points is just preparation for actual SDF exectuion here.        
            .ToListAsync();
        
        var historyOne = readings.Select(x => new ReadingDTO
        {
            SourceId = sourceId,
            SourceName = source.Name,

            Timestamp = x.Timestamp,
            Status = x.Status,
            RPM = x.RPM,
            Power = x.Power,
            Temperature = x.Temperature
        }).ToList();

        return Ok(historyOne);
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