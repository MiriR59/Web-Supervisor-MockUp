using WSV.Api.Models;

namespace WSV.Api.Services;

public class ReadingCacheService : IReadingCacheService
{
    private readonly ILogger<ReadingCacheService> _logger;
    private static readonly TimeSpan Retention = TimeSpan.FromSeconds(60);
    private readonly Dictionary<int, Queue<SourceReading>> _buffers = new();
    private readonly Dictionary<int, SourceReading> _latest = new();
    private readonly object _lock = new();

    public ReadingCacheService(
        ILogger<ReadingCacheService> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<SourceReading> GetRecentOne(int sourceId)
    {
        lock (_lock)
        {
            ExpireOldReadingsLock(sourceId, DateTimeOffset.UtcNow);

            if (_buffers.TryGetValue(sourceId, out var q) && q.Count > 0)
                return q.ToList();

            return Array.Empty<SourceReading>();
        }
    }

    public SourceReading? GetLatestOne(int sourceId)
    {
        lock (_lock)
        {
            return _latest.TryGetValue(sourceId, out var r) ? r : null;
        }
    }
    public void SetRecentReading(SourceReading reading)
    {
        lock (_lock)
        {
            var now = DateTimeOffset.UtcNow;

            _latest[reading.SourceId] = reading;

            if (!_buffers.TryGetValue(reading.SourceId, out var q))
            {
                q = new Queue<SourceReading>();
                _buffers[reading.SourceId] = q;
            }

            q.Enqueue(reading);

            _logger.LogInformation(
                "CACHE sanity: count={count}, newest={newest}",
                q.Count,
                q.Last().Timestamp.ToString("O")
            );

            ExpireQueueLock(q, now);
        }
    }

    private void ExpireOldReadingsLock(int sourceId, DateTimeOffset now)
    {
        if (!_buffers.TryGetValue(sourceId, out var q))
            return;

        ExpireQueueLock(q, now);

        if (q.Count == 0)
            _buffers.Remove(sourceId);
    }

    private static void ExpireQueueLock(Queue<SourceReading> q, DateTimeOffset now)
    {
        var cutoff = now - Retention;

        while (q.Count > 0)
        {
            var oldest = q.Peek();
            if (oldest.Timestamp >= cutoff)
                break;

            q.Dequeue();
        }
    }
}   