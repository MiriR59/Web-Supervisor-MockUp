using WSV.Api.Models;

namespace WSV.Api.Services;

public interface ILastReadingService
{
    SourceReading? GetOne(int sourceId);

    IEnumerable<SourceReading> GetAll();

    void SetLastReading(SourceReading reading); 
}