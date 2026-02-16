namespace WSV.Api.Configuration;

public class WriterOptions
{
    public TimeSpan TickInterval { get; set; } = TimeSpan.FromSeconds(10);
    public int NormalBatch { get; set; } = 60;
    public int SlowBatch { get; set; } = 3;
}