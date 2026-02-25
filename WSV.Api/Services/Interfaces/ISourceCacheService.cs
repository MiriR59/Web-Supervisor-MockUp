using WSV.Api.Models;

namespace WSV.Api.Services;

public interface ISourceCacheService
{
    IReadOnlyList<Source> GetAllSources();

    Task ReloadSourcesAsync();

}