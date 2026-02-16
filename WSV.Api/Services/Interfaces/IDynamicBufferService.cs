using WSV.Api.Models;

namespace WSV.Api.Services;

public interface IDynamicBufferService
{
    int BufferedCount { get;}
    void Enqueue(SourceReading reading);
    SourceReading? Dequeue();
}