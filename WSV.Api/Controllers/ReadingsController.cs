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
    [HttpGet("latest")]
    public IActionResult GetLatestAll()
    {
        var latestAll = _lastReadingService.GetAll();

        return Ok(latestAll);
    }

    //GET only the latest reading for one particular source /api/readings/source/{SourceId}/latest
    [HttpGet("source/{sourceId}/latest")]
    public IActionResult GetLatestOne(int sourceId)
    {
        var latestOne = _lastReadingService.GetOne(sourceId);

        return Ok(latestOne);
    }

    //GET history of single source from Time to Time /api/readings/source/{SourceId}?...
    //From and To will be added as query information
    //[HttpGet("source/{sourceId}")]
    //public async task <IActionResult> GetHistoryOne(
    //    int sourceId,
    //    [FromQuery] DateTime? from,
    //    [FromQuery] DateTime? to
    //)
    //{


    //}

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