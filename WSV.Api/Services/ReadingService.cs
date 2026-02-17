using WSV.Api.Data;
using WSV.Api.Models;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<ReadingDto>> GetHistoryAsync(
        int sourceId, DateTimeOffset? from, DateTimeOffset? to, int? limit)
    {
        var query = _context.SourceReadings
            .AsNoTracking()
            .Where(t => t.SourceId == sourceId);

        var cacheReadings = _readingCacheService.GetRecentOne(sourceId);
        IEnumerable<SourceReading> cacheFiltered = cacheReadings;

        if(from.HasValue)
        {
            query = query.Where(u => u.Timestamp >= from.Value);    
        }

        if(to.HasValue)
        {
            query = query.Where(v => v.Timestamp < to.Value);
            cacheFiltered = cacheFiltered.Where(r => r.Timestamp < to.Value);
        }

        // Hard cap of 5000 readings to prevent accidental overloading
        // Basic soft cap stays at 1000
        var take = Math.Clamp(limit ?? 1000, 1, 5000);
        
        var readings = await query 
            .OrderByDescending(r => r.Timestamp)  
            .Take(take)  
            .ToListAsync();

        var merged = new Dictionary<DateTimeOffset, ReadingDto>();
        foreach (var d in cacheFiltered.Select(MapToDto))
            merged[d.Timestamp] = d;
        foreach (var d in readings.Select(MapToDto))
            merged[d.Timestamp] = d;

        return merged.Values
            .OrderByDescending(r => r.Timestamp)
            .Take(take)
            .ToList();
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