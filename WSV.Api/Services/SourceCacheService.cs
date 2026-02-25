using Microsoft.EntityFrameworkCore;
using WSV.Api.Data;
using WSV.Api.Models;

namespace WSV.Api.Services;

public class SourceCacheService: ISourceCacheService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private List<Source> _sources = new();
    private readonly object _lock = new();
    
    public SourceCacheService(
        IServiceScopeFactory scopeFactory
    )
    {
        _scopeFactory = scopeFactory;
        
    }

    public IReadOnlyList<Source> GetAllSources()
    {
        lock (_lock)
        {
            return _sources.ToList();
        }
    }

    public async Task ReloadSourcesAsync()
    {
        List<Source> newSources;
        using(var scope = _scopeFactory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            newSources = await db.Sources.AsNoTracking().ToListAsync();
        }

        lock (_lock)
        {
            _sources = newSources;
        }
    }
}