using WSV.Api.Data;
using WSV.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WSV.Api.Services;

public class ReadingService : IReadingService
{
    private readonly AppDbContext _context;
    private readonly IReadingCacheService _readingCacheService;

    public ReadingService(
        AppDbContext context,
        IReadingCacheService readingCacheService)
    {
        _context = context;
        _readingCacheService = readingCacheService;
    }

    public async Task<List<ReadingDto>> GetHistoryAsync(int sourceId, DateTimeOffset? from, DateTimeOffset? to, int? limit)
    {
        var end = to ?? DateTimeOffset.UtcNow;
        var take = Math.Clamp(limit ?? 1000, 1, 5000);

        var query = _context.SourceReadings
            .AsNoTracking()
            .Where(t => t.SourceId == sourceId)
            .Where(v => v.Timestamp < end);

        if(from.HasValue)
            query = query.Where(u => u.Timestamp >= from);

        var count = await query.CountAsync();

        if(count > take)
            return await GetAggregatedHistoryAsync(sourceId, from, end, take, );
        
        return await GetRawHistoryAsync(sourceId, from, end, take);
    }

    public async Task<List<ReadingDto>> GetRawHistoryAsync(int sourceId, DateTimeOffset? from, DateTimeOffset to, int limit)
    {
        var query = _context.SourceReadings
            .AsNoTracking()
            .Where(t => t.SourceId == sourceId)
            .Where(v => v.Timestamp < to);

        if(from.HasValue)
        {
            query = query.Where(u => u.Timestamp >= from.Value);    
        }
        
        var readings = await query   
            .OrderByDescending(r => r.Timestamp)  
            .Take(limit)  
            .ToListAsync();

        var cacheReadings = _readingCacheService.GetRecentOne(sourceId);
        IEnumerable<SourceReading> cacheFiltered = cacheReadings;
        cacheFiltered = cacheFiltered.Where(r => r.Timestamp < to);

        var merged = new Dictionary<DateTimeOffset, ReadingDto>();
        foreach (var d in cacheFiltered.Select(MapToDto))
            merged[d.Timestamp] = d;
        foreach (var d in readings.Select(MapToDto))
            merged[d.Timestamp] = d;

        return merged.Values
            .OrderByDescending(r => r.Timestamp)
            .Take(limit)
            .ToList();
    }

    public async Task<List<ReadingDto>> GetAggregatedHistoryAsync(int sourceId, DateTimeOffset? from, DateTimeOffset to, int limit)
    {
        // ADD RAW SQL QUERY, USING LINQ IS SUPER SLOW HERE, IF I HAD BIG DATA IT WOULD BE SLOW AF
        // CHANGE ALL BELOW
        var query = _context.SourceReadings
            .AsNoTracking()
            .Where(t => t.SourceId == sourceId)
            .Where(u => u.Timestamp >= from)
            .Where(v => v.Timestamp < to);

        var readingsCount = query.Count();
        int bucketSize = readingsCount / limit;
        if(readingsCount % limit != 0)
            bucketSize ++;

        double interval = (from.ToUnixTimeSeconds() - to.ToUnixTimeSeconds()) / bucketSize; // IS THIS ACCURATE? OR WILL I LOSE SOME INFO DUE TO ROUNDING>?

        for(int i = 0; i < bucketSize; i++)
        {
            var dtoInterval = query
                .Where(u => u.Timestamp >= from)
                .Where(v => v.Timestamp < to);
            var dtoPower = dtoInterval.Average(w => w.Power);
            var dtoRpm = dtoInterval.Average(w => w.RPM);
            var dtoTemperature = dtoInterval.Average(w => w.Temperature);

            ReadingDto dtoOut = new ReadingDto
            {
                SourceId = sourceId,
                Timestamp = reading.Timestamp,
                Status = reading.Status,
                RPM = dtoRpm.,
                Power = reading.Power,
                Temperature = reading.Temperature
            };
        }  
    }

    public async Task<LagDto> GetLagAsync(int sourceId)
    {
        var latestGenerated = _readingCacheService.GetLatestOne(sourceId);
        if(latestGenerated is null)
            return new LagDto{
                SourceId = sourceId,
                State = LagState.NoLiveData};

        var latestDb = await _context.SourceReadings
            .AsNoTracking()
            .Where(r => r.SourceId == sourceId)
            .MaxAsync(r => (DateTimeOffset?)r.Timestamp);

        if(latestDb is null)
            return new LagDto{
                SourceId = sourceId,
                State = LagState.DbEmpty,
                LatestGenerated = latestGenerated.Timestamp};

        var lag = latestGenerated.Timestamp - latestDb.Value;
        var lagOut = Math.Max(0, lag.TotalSeconds);

        return new LagDto{
            SourceId = sourceId,
            State = LagState.Ok,
            LatestGenerated = latestGenerated.Timestamp,
            LatestDb = latestDb.Value,
            DbLag = lagOut};
    }

    public async Task<Source?> GetPublicSourceAsync(int sourceId)
    {
        return await _context.Sources
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == sourceId && p.IsPublic);
    }

    public async Task<Source?> GetSourceAsync(int sourceId)
    {
        return await _context.Sources
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == sourceId);
    }

    private static ReadingDto MapToDto(SourceReading reading) => new ReadingDto
    {
        SourceId = reading.SourceId,
        Timestamp = reading.Timestamp,
        Status = reading.Status,
        RPM = reading.RPM,
        Power = reading.Power,
        Temperature = reading.Temperature
    };
}