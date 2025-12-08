using WSV.Api.Models;

namespace WSV.Api.Services;

// Implementation in accordance with contract ISource...
public class SourceBehaviourService : ISourceBehaviourService
{
    // Single shared random for all readings
    private readonly Random _random = new();
    private readonly ILastReadingService _lastReadingService;

    public SourceBehaviourService(ILastReadingService lastReadingService)
    {
        _lastReadingService = lastReadingService;
    }

    public SourceReading GenerateReading(Source source, DateTime now)
    {
        var t = (now - DateTime.UnixEpoch).TotalSeconds;
        var previous = _lastReadingService.GetOne(source.Id);

        if (source.IsEnabled == false)
        {
            return GenerateStoppedReading(source, now, previous);
        }

        return source.BehaviourProfile switch
        {
            "Stable" => GenerateStableReading(source, now),
            "Wave" => GenerateWaveReading(source, now, t),
            "Spiky" => GenerateSpikyReading(source, now),

            _ => GenerateStableReading(source, now),
        };
    }

    // Behaviour functions called above
    private SourceReading GenerateStoppedReading(Source source, DateTime now, SourceReading? previous)
    {
        int lastRPM = previous?.RPM ?? 0;
        int newRPM = Math.Max(0, lastRPM - 300);

        double lastTemp = previous?.Temperature ?? 20.0;
        double newTemp = Math.Max(20.0, lastTemp - 5.0);

        return new SourceReading
        {
            SourceId = source.Id,
            Timestamp = now,
            Status = "Stopped",
            RPM = newRPM,
            Power = 0,
            Temperature = newTemp,
        };
    }
    private SourceReading GenerateStableReading(Source source, DateTime now)
    {
        // Stable source with small noise levels
        int baseRpm = 1500;
        int basePower = 60;
        double baseTemp = 85;

        int rpm = baseRpm + _random.Next(-50, 51);
        int power = basePower + _random.Next(-4, 5);
        double temp = baseTemp + (_random.NextDouble() * 4 - 2);

        return new SourceReading
        {
            SourceId = source.Id,
            Timestamp = now,
            Status = "Running",
            RPM = rpm,
            Power = power,
            Temperature = temp,
        };
    }

    private SourceReading GenerateWaveReading(Source source, DateTime now, double t)
    {
        // Add sine wave source with small noise levels
        throw new NotImplementedException();
    }

    private SourceReading GenerateSpikyReading(Source source, DateTime now)
    {
        // Add linear behaviour with random spikes
        throw new NotImplementedException();
    }
}