using WSV.Api.Models;

namespace WSV.Api.Services;

public interface ISourceBehaviourService
{
    SourceReading GenerateReading(Source source, DateTime now);
}