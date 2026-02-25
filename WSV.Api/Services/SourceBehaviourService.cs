using WSV.Api.Models;

namespace WSV.Api.Services;

// Implementation in accordance with contract ISource...
public class SourceBehaviourService : ISourceBehaviourService
{
    // Single shared random for all readings
    private readonly Random _random = new();
    private readonly IReadingCacheService _readingCacheService;

    public SourceBehaviourService(IReadingCacheService readingCacheService)
    {
        _readingCacheService =  readingCacheService;
    }

    public SourceReading GenerateReading(Source source, DateTimeOffset now)
    {
        var t = (now - DateTime.UnixEpoch).TotalSeconds;
        var previous =  _readingCacheService.GetLatestOne(source.Id);

        if (source.IsEnabled == false)
        {
            return GenerateStoppedReading(source, now, previous);
        }

        return source.Behaviour switch
        {
            BehaviourProfile.Stable => GenerateStableReading(source, now),
            BehaviourProfile.Wave => GenerateWaveReading(source, now, t),
            BehaviourProfile.Spiky => GenerateSpikyReading(source, now),

            _ => GenerateStableReading(source, now),
        };
    }

    // Behaviour functions called above
    private SourceReading GenerateStoppedReading(Source source, DateTimeOffset now, SourceReading? previous)
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
    private SourceReading GenerateStableReading(Source source, DateTimeOffset now)
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

    private SourceReading GenerateWaveReading(Source source, DateTimeOffset now, double t)
    {
        int baseRpm = 1500;
        int basePower = 60;
        double baseTemp = 85;
        double period = 120;
        double omega = 2 * Math.PI / period;
        double x = omega * t;
        double sine = Math.Sin(x);

        int rpm = baseRpm + (int)(200 * sine);
        int power = basePower + (int)(10 * sine);
        double temp = baseTemp + 5.0 * sine;

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

    private SourceReading GenerateSpikyReading(Source source, DateTimeOffset now)
    {
        int baseRpm = 1500;
        int basePower = 60;
        double baseTemp = 85;

        int rpm = baseRpm + _random.Next(-500, 501);
        int power = basePower +_random.Next(-30, 31);
        double temp = baseTemp + (_random.NextDouble() * 40 - 20);

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
}