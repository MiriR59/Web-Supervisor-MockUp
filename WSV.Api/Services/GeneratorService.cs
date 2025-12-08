using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SQLitePCL;
using WSV.Api.Data;

namespace WSV.Api.Services;

// Background services rungs in the bg and periodically generates
public class GeneratorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISourceBehaviourService _behaviourService;
    private readonly ILastReadingService _lastReadingService;

    public GeneratorService(
        IServiceScopeFactory scopeFactory,
        ISourceBehaviourService behaviourService,
        ILastReadingService lastReadingService)
    {
        _scopeFactory = scopeFactory;
        _behaviourService = behaviourService;
        _lastReadingService = lastReadingService;
    }

    // Following method is called on the start and runs until shutdown
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Loops until stopped by cancellation
        while (!stoppingToken.IsCancellationRequested)
        {
            using(var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var now = DateTime.UtcNow;

                // Load all sources from DB
                var sources = await db.Sources.ToListAsync(stoppingToken);

                // Generate reading for each source
                foreach(var source in sources)
                {
                    var reading = _behaviourService.GenerateReading(source, now);

                    // Add to DbContext
                    db.SourceReadings.Add(reading);

                    // Update cache
                    _lastReadingService.SetLastReading(reading);
                }

                // Save everything at once
                await db.SaveChangesAsync(stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}