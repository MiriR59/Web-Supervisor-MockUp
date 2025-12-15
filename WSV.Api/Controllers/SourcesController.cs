using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WSV.Api.Data;

namespace WSV.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SourcesController : ControllerBase
{
    private readonly AppDbContext _context;

    public SourcesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sources = await _context.Sources
            .OrderBy(s => s.Id)
            .ToListAsync();

        var dtoSources = sources.Select(s => new SourceDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            IsEnabled = s.IsEnabled,
            BehaviourProfile = s.BehaviourProfile
        }).ToList();

        return Ok(dtoSources);
    }

    [HttpPatch("{sourceId}/enabled")]
    public async Task<IActionResult> SetEnabled(int sourceId, [FromBody] SetEnabledDto dto)
    {
        var source = await _context.Sources.FindAsync(sourceId);

        if (source == null)
            return NotFound();

        source.IsEnabled = dto.IsEnabled;
        await _context.SaveChangesAsync();

        return Ok(source.IsEnabled);
    }
}
