using WSV.Api.Models;

namespace WSV.Api.Services;

public class LastReadingService : ILastReadingService
{
    // Dictionary, where I have values of SourceReading for each int(SourceId) entry
    // Super fast, 1 key -> 1 value, no need to search through it
    // 1    last reading of source 1
    // 2    last reading of source 2
    // 3    last reading of source 3
    private readonly Dictionary<int, SourceReading> _lastReadings = new();

    public SourceReading? GetOne(int sourceId)
    {
        // If TryGetValue succeeds, return reading, otherwise return null.
        // This goes back to the SourceReading? enabling null value
        if(_lastReadings.TryGetValue(sourceId, out var reading))
        {
            return reading;
        }
    
        return null;
    }

    public IEnumerable<SourceReading> GetAll()
    {
        return _lastReadings.Values.ToList();
    }

    public void SetLastReading(SourceReading reading)
    {
        _lastReadings[reading.SourceId] = reading;
    }
}