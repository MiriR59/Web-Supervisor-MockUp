using WSV.Api.Models;

namespace WSV.Api.Services;

public class ReadingCacheService : IReadingCacheService
{
    private static readonly TimeSpan Retention = TimeSpan.FromSeconds(60);
    private readonly Dictionary<int, Queue<SourceReading>> _buffers = new();
    private readonly Dictionary<int, SourceReading> _latest = new();
    private readonly object _lock = new();

    public IReadOnlyList<SourceReading> GetRecentOne(int sourceId)
    {
        lock (_lock)
        {
            ExpireOldReadingsLock(sourceId, DateTime.UtcNow);

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
            var now = DateTime.UtcNow;

            _latest[reading.SourceId] = reading;

            if (!_buffers.TryGetValue(reading.SourceId, out var q))
            {
                q = new Queue<SourceReading>();
                _buffers[reading.SourceId] = q;
            }

            q.Enqueue(reading);

            ExpireQueueLock(q, now);
        }
    }

    private void ExpireOldReadingsLock(int sourceId, DateTime now)
    {
        if (!_buffers.TryGetValue(sourceId, out var q))
            return;

        ExpireQueueLock(q, now);

        if (q.Count == 0)
            _buffers.Remove(sourceId);
    }

    private static void ExpireQueueLock(Queue<SourceReading> q, DateTime now)
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