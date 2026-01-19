using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WSV.Api.Data;
using WSV.Api.Models;

namespace WSV.Api.Services;

public class DbWriterService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IReadingBufferService _buffer;
    private readonly ILogger<DbWriterService> _logger;

    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(30);

    private const int NormalBatch = 15;
    private const int SlowBatch = 3;

    private static readonly TimeSpan CycleLength = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan SlowDuration = TimeSpan.FromMinutes(2);

    private readonly DateTime _startUtc = DateTime.UtcNow;

    public DbWriterService(
        IServiceScopeFactory scopeFactory,
        IReadingBufferService buffer,
        ILogger<DbWriterService> logger)
    {
        _scopeFactory = scopeFactory;
        _buffer = buffer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TickInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var batchSize = IsSlowMode(DateTime.UtcNow) ? SlowBatch : NormalBatch;

            var batch = new List<SourceReading>(capacity: batchSize);

            for (int i = 0; i < batchSize; i++)
            {
                if (!_buffer.TryDequeue(out var reading))
                    break;

                batch.Add(reading);
            }

            if (batch.Count == 0)
                continue;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                db.SourceReadings.AddRange(batch);
                await db.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("DB writer tick: target={Target}, wrote={Wrote}, buffered={Buffered}, mode={Mode}",
                    batchSize, batch.Count, _buffer.ApproximateCount, batchSize == SlowBatch ? "SLOW" : "NORMAL");

            }
            catch (Exception ex)
            {
                // Error logging
                _logger.LogError(ex, "DB writer failed to save batch of {Count} readings,", batch.Count);
            }
        }
    }
    // %  returns what remains after the division, slowmode at the start of each 5 min cycle
    private bool IsSlowMode(DateTime nowUtc)
    {
        var elapsed = nowUtc - _startUtc;
        var withinCycle = TimeSpan.FromSeconds(elapsed.TotalSeconds % CycleLength.TotalSeconds);

        return withinCycle < SlowDuration;
    }
}