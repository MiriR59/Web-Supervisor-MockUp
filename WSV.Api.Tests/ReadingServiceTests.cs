using Microsoft.EntityFrameworkCore;
using Moq;
using WSV.Api.Data;
using WSV.Api.Models;
using WSV.Api.Services;
using Xunit.Sdk;

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
        // Create empty in-memory db - no readings needed for this
        var context = CreateContext();

        // --- ARANGE --- set up everything the service needs
        // Create fake cache that returns null for any sourceId
        var mockCache = new Mock<IReadingCacheService>();
        mockCache
            .Setup(c => c.GetLatestOne(1))
            .Returns((SourceReading?)null); // simulates empty cache

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
        var context = CreateContext();

        var fakeReading = new SourceReading
        {
            SourceId = 1,
            Timestamp = DateTimeOffset.Now
        };

        var mockCache = new Mock<IReadingCacheService>();
        mockCache
            .Setup(c => c.GetLatestOne(1))
            .Returns(fakeReading);
        
        var service = new ReadingService(context, mockCache.Object);

        var result = await service.GetLagAsync(1);

        Assert.Equal(LagState.DbEmpty, result.State);
        Assert.Equal(fakeReading.Timestamp, result.LatestGenerated);
    }

    [Fact]
    public async Task GetLagAsync_WhenAllDataAvailable_ReturnsLagDto()
    {
        DateTimeOffset timestampUnited = DateTimeOffset.UtcNow;

        var context = CreateContext();
        context.SourceReadings.Add(new SourceReading
        {
            SourceId = 1,
            Timestamp = timestampUnited.AddSeconds(-10)
        });
        await context.SaveChangesAsync();

        var fakeReading = new SourceReading
        {
            SourceId = 1,
            Timestamp = timestampUnited
        };

        var mockCache = new Mock<IReadingCacheService>();
        mockCache
            .Setup(c => c.GetLatestOne(1))
            .Returns(fakeReading);
        
        var service = new ReadingService(context, mockCache.Object);

        var result = await service.GetLagAsync(1);

        Assert.Equal(LagState.Ok, result.State);
        Assert.Equal(10, result.DbLag);
    }

    [Fact]
    public async Task GetPublicSourceAsync_WhenIdDoesNotMatchAndIsPublicIsTrue_ReturnsNull()
    {
        var context = CreateContext();
        context.Sources.Add(new Source
        {
            Id = 11,
            Name = "Source11",
            IsPublic = true
        });
        await context.SaveChangesAsync();

        var mockCache = new Mock<IReadingCacheService>();

        var service = new ReadingService(context, mockCache.Object);

        var result = await service.GetPublicSourceAsync(22);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPublicSourceAsync_WhenIdMatchesAndIsPublicIsFalse_ReturnsNull()
    {
        var context = CreateContext();
        context.Sources.Add(new Source
        {
            Id = 11,
            Name = "Source11",
            IsPublic = false
        });
        await context.SaveChangesAsync();

        var mockCache = new Mock<IReadingCacheService>();

        var service = new ReadingService(context, mockCache.Object);

        var result = await service.GetPublicSourceAsync(11);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPublicSourceAsync_WhenIdMatchesAndIsPublicIsTrue_ReturnsSource()
    {
        var context = CreateContext();
        context.Sources.Add(new Source
        {
            Id = 11,
            Name = "Source11",
            IsPublic = true
        });
        await context.SaveChangesAsync();

        var mockCache = new Mock<IReadingCacheService>();

        var service = new ReadingService(context, mockCache.Object);

        var result = await service.GetPublicSourceAsync(11);

        Assert.NotNull(result);
        Assert.Equal("Source11", result.Name);
    }

    [Fact]
    public async Task GetSourceAsync_WhenSourceDoesNotExist_ReturnsNull()
    {
        var context = CreateContext();
        context.Sources.Add(new Source
        {
            Id = 11,
            Name = "Source11"
        });
        await context.SaveChangesAsync();

        var mockCache = new Mock<IReadingCacheService>();

        var service = new ReadingService(context, mockCache.Object);

        var result = await service.GetSourceAsync(22);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetSourceAsync_WhenSourceExists_ReturnsSource()
    {
        var context = CreateContext();
        context.Sources.Add(new Source
        {
            Id = 11,
            Name = "Source11"
        });
        await context.SaveChangesAsync();

        var mockCache = new Mock<IReadingCacheService>();

        var service = new ReadingService(context, mockCache.Object);

        var result = await service.GetSourceAsync(11);

        Assert.NotNull(result);
        Assert.Equal("Source11", result.Name);
    }
}
