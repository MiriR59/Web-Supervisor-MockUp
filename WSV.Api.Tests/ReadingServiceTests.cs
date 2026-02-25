using Microsoft.EntityFrameworkCore;
using Moq;
using WSV.Api.Data;
using WSV.Api.Models;
using WSV.Api.Services;

namespace WSV.Api.Tests;

public class ReadingServiceTests
{
    // Helper that creates fresh in-memory DB for each test
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb_" + Guid.NewGuid())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetLagAsync_WhenNoLiveData_ReturnsNoLiveDataState()
    {
        // --- ARANGE --- set up everything the service needs
        // Create fake cache that returns null for any sourceId
        var mockCache = new Mock<IReadingCacheService>();
        mockCache
            .Setup(c => c.GetLatestOne(1))
            .Returns((SourceReading?)null); // simulates empty cache

        // Create empty in-memory db - no readings needed for this
        var context = CreateContext();

        // Wire up the service with fake dependencies
        var service = new ReadingService(context, mockCache.Object);

        // --- ACT --- call the method we are testing
        var result = await service.GetLagAsync(1);

        // --- ASSERT --- verify the result against what we expect
        Assert.Equal(LagState.NoLiveData, result.State);
        Assert.Equal(1, result.SourceId);
    }

    [Fact]
    public async Task GetLagAsync_WhenNoDbData_ReturnsDbEmptyState()
    {
        var fakeReading = new SourceReading
        {
            SourceId = 1,
            Timestamp = DateTimeOffset.Now
        };

        var mockCache = new Mock<IReadingCacheService>();
        mockCache
            .Setup(c => c.GetLatestOne(1))
            .Returns(fakeReading);
        
        var context = CreateContext();
        var service = new ReadingService(context, mockCache.Object);

        var result = await service.GetLagAsync(1);

        Assert.Equal(LagState.DbEmpty, result.State);
        Assert.Equal(fakeReading.Timestamp, result.LatestGenerated);
    }

    [Fact]
    public async Task GetLagAsync_WhenAllDataAvailable_ReturnsLagDto()
    {
         DateTimeOffset timestampUnited = DateTimeOffset.UtcNow;

        var fakeReading = new SourceReading
        {
            SourceId = 1,
            Timestamp = timestampUnited
        };

        var mockCache = new Mock<IReadingCacheService>();
        mockCache
            .Setup(c => c.GetLatestOne(1))
            .Returns(fakeReading);
        
        var context = CreateContext();
        context.SourceReadings.Add(new SourceReading
        {
            SourceId = 1,
            Timestamp = timestampUnited.AddSeconds(-10)
        });
        await context.SaveChangesAsync();

        var service = new ReadingService(context, mockCache.Object);

        var result = await service.GetLagAsync(1);

        Assert.Equal(LagState.Ok, result.State);
        Assert.Equal(10, result.DbLag);
    }
}
