using System.Diagnostics.CodeAnalysis;
using WSV.Api.Models;

namespace WSV.Api.Services;

public interface IReadingBufferService
{
    int ApproximateCount { get;}
    ValueTask EnqueueAsync(SourceReading reading, CancellationToken ct);
    ValueTask<SourceReading> DequeueAsync(CancellationToken ct);
    bool TryDequeue([NotNullWhen(true)] out SourceReading? reading);
}