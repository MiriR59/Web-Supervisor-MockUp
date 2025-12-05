using WSV.Api.Models;

namespace WSV.Api.Services;

public interface ILastReadingService
{
    SourceReading? GetLastReading(int sourceId);

    void SetLastReading(SourceReading reading); 
}