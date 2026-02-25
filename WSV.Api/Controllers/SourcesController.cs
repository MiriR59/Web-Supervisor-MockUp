using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WSV.Api.Data;
using WSV.Api.Services;

namespace WSV.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SourcesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ISourceCacheService _sourceCacheService;

    public SourcesController(
        AppDbContext context,
        ISourceCacheService sourceCacheService)
    {
        _context = context;
        _sourceCacheService = sourceCacheService;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewAllSources")]
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
            Behaviour = s.Behaviour.ToString()
        }).ToList();

        return Ok(dtoSources);
    }

    [HttpPatch("{sourceId}/enabled")]
    [Authorize(Policy = "CanToggleSources")]
    public async Task<IActionResult> SetEnabled(int sourceId, [FromBody] SetEnabledDto dto)
    {
        var source = await _context.Sources.FindAsync(sourceId);

        if (source == null)
            return NotFound();

        source.IsEnabled = dto.IsEnabled;
        await _context.SaveChangesAsync();
        await _sourceCacheService.ReloadSourcesAsync();

        return Ok(source.IsEnabled);
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublic()
    {
        var sources = await _context.Sources
            .AsNoTracking()
            .Where(p => p.IsPublic == true)
            .OrderBy(p => p.Id)
            .ToListAsync();
            
        var dtoPublic = sources.Select(s => new SourceDto
        {   Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            IsEnabled = s.IsEnabled,
            Behaviour = s.Behaviour.ToString()
        }).ToList();

        return Ok(dtoPublic);
    }
}
