using Microsoft.EntityFrameworkCore;
using WSV.Api.Data;
using WSV.Api.Models;

namespace WSV.Api.Services;

// Background services rungs in the bg and periodically generates
public class GeneratorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISourceBehaviourService _behaviourService;
    private readonly IReadingCacheService _readingCacheService;
    private readonly IReadingBufferService _readingBufferService;

    public GeneratorService(
        IServiceScopeFactory scopeFactory,
        ISourceBehaviourService behaviourService,
        IReadingCacheService readingCacheService,
        IReadingBufferService readingBufferService)
    {
        _scopeFactory = scopeFactory;
        _behaviourService = behaviourService;
        _readingCacheService = readingCacheService;
        _readingBufferService = readingBufferService;
    }

    // Following method is called on the start and runs until shutdown
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Loops until stopped by cancellation
        while (!stoppingToken.IsCancellationRequested)
        {
            // Unnecessary to load sources again and again, add reactivity to Enable/disable
            List<Source> sources;
            using(var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                sources = await db.Sources.AsNoTracking().ToListAsync(stoppingToken);
            }

            // Generate reading for each source and enqueue it
            foreach(var source in sources)
            {
                var now = DateTimeOffset.UtcNow;
                var reading = _behaviourService.GenerateReading(source, now);

                // Update cache
                _readingCacheService.SetRecentReading(reading);

                // Queue reading in the channel
                await _readingBufferService.EnqueueAsync(reading, stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}