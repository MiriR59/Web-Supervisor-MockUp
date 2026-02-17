namespace WSV.Api.Configuration;

public class CacheOptions
{
    public TimeSpan Retention { get; set; } = TimeSpan.FromSeconds(60);
    public int WarningThreshold { get; set; } = 150;
}