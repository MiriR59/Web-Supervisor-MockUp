using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WSV.Api.Data;
using WSV.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.IdentityModel.Tokens;
using WSV.Api.Models;
using System.Net;
using Npgsql;

namespace WSV.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReadingsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IReadingCacheService _readingCacheService;
    private readonly IAuthorizationService _authorization;
    private readonly ILogger<ReadingsController> _logger;

    public ReadingsController(
        AppDbContext context,
        IReadingCacheService readingCacheService,
        IAuthorizationService authorization,
        ILogger<ReadingsController> logger)
    {
        _context = context;
        _readingCacheService = readingCacheService;
        _authorization = authorization;
        _logger = logger;
    }   

    //GET history of single source from Time to Time /api/readings/source/{SourceId}?...
    //From and To will be added as query information
    //Pure DB query, DATA-LAKE like
    [HttpGet("source/{sourceId}")]
    [Authorize(Policy = "CanViewAllSources")]
    public async Task <IActionResult> GetHistoryOne(
        int sourceId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int? limit
        )
    {
        var query = _context.SourceReadings
            .AsNoTracking()
            .Where(t => t.SourceId == sourceId);

        var cacheReadings = _readingCacheService.GetRecentOne(sourceId);
        IEnumerable<SourceReading> cacheFiltered = cacheReadings;

        if(from.HasValue)
        {
            query = query.Where(u => u.Timestamp >= from.Value);    
        }

        if(to.HasValue)
        {
            query = query.Where(v => v.Timestamp < to.Value);
            cacheFiltered = cacheFiltered.Where(r => r.Timestamp < to.Value);
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

        var dbDto = readings.Select(x => new ReadingDto
        {
            SourceId = x.SourceId,
            Timestamp = x.Timestamp,
            Status = x.Status,
            RPM = x.RPM,
            Power = x.Power,
            Temperature = x.Temperature
        });
        var cacheDto = cacheFiltered.Select(x => new ReadingDto
        {
            SourceId = x.SourceId,
            Timestamp = x.Timestamp,
            Status = x.Status,
            RPM = x.RPM,
            Power = x.Power,
            Temperature = x.Temperature
        });

        var merged = new Dictionary<DateTimeOffset, ReadingDto>();
        foreach (var d in cacheDto)
            merged[d.Timestamp] = d;
        foreach (var d in dbDto)
            merged[d.Timestamp] = d;

        var dto = merged.Values
            .OrderByDescending(r => r.Timestamp)
            .Take(take)
            .ToList();

        return Ok(dto);
    }

    [HttpGet("public/source/{sourceId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicOne(
        int sourceId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int? limit
        )
    {
        var sourceCheck = await _context.Sources
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == sourceId && p.IsPublic);
        if(sourceCheck is null)
            return NotFound();

        var query = _context.SourceReadings
            .AsNoTracking()
            .Where(t => t.SourceId == sourceId);

        var cacheReadings = _readingCacheService.GetRecentOne(sourceId);
        IEnumerable<SourceReading> cacheFiltered = cacheReadings;

        if(from.HasValue)
        {
            query = query.Where(u => u.Timestamp >= from.Value);    
            cacheFiltered = cacheFiltered.Where(r => r.Timestamp >= from.Value);
        }

        if(to.HasValue)
        {
            query = query.Where(v => v.Timestamp < to.Value);
            cacheFiltered = cacheFiltered.Where(r => r.Timestamp < to.Value);
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

        var dbDto = readings.Select(x => new ReadingDto
        {
            SourceId = x.SourceId,
            Timestamp = x.Timestamp,
            Status = x.Status,
            RPM = x.RPM,
            Power = x.Power,
            Temperature = x.Temperature
        });
        var cacheDto = cacheFiltered.Select(x => new ReadingDto
        {
            SourceId = x.SourceId,
            Timestamp = x.Timestamp,
            Status = x.Status,
            RPM = x.RPM,
            Power = x.Power,
            Temperature = x.Temperature
        });

        var merged = new Dictionary<DateTimeOffset, ReadingDto>();
        foreach (var d in cacheDto)
            merged[d.Timestamp] = d;
        foreach (var d in dbDto)
            merged[d.Timestamp] = d;

        var dto = merged.Values
            .OrderByDescending(r => r.Timestamp)
            .Take(take)
            .ToList();

        return Ok(dto);
    }

    [HttpGet("source/{sourceId}/lag")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLagOne(
        int sourceId
    )
    {
        var sourceCheck = await _context.Sources
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == sourceId);

        if(sourceCheck is null)
            return NotFound();
        
        if(!sourceCheck.IsPublic)
        {
            var auth = await _authorization.AuthorizeAsync(User, "CanViewAllSources");
            if(!auth.Succeeded)
                return NotFound();
        }

        var latestGenerated = _readingCacheService.GetLatestOne(sourceId);
        if(latestGenerated is null)
            return Ok(new LagDto
            {
                SourceId = sourceId,
                State = LagState.NoLiveData
            });

        var latestDb = await _context.SourceReadings
            .AsNoTracking()
            .Where(r => r.SourceId == sourceId)
            .MaxAsync(r => (DateTimeOffset?)r.Timestamp);

        if(latestDb is null)
            return Ok(new LagDto
            {
                SourceId = sourceId,
                State = LagState.DbEmpty,
                LatestGenerated = latestGenerated.Timestamp
            });

        var lag = latestGenerated.Timestamp - latestDb.Value;
        var lagOut = Math.Max(0, lag.TotalSeconds);

        return Ok (new LagDto
        {
            SourceId = sourceId,
            State = LagState.Ok,
            LatestGenerated = latestGenerated.Timestamp,
            LatestDb = latestDb.Value,
            DbLag = lagOut
        });
    }
}