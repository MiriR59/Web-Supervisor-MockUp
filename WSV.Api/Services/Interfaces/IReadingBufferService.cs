using WSV.Api.Models;

namespace WSV.Api.Services;

public interface IReadingBufferService
{
    int BufferedCount { get;}
    void Enqueue(SourceReading reading);
    SourceReading? Dequeue();
}