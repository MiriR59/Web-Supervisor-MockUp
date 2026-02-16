namespace WSV.Api.Configuration;

public class BufferOptions
{
    public int CapacityPrimary { get; set; } = 200;
    public int CapacityOverflow { get; set; } = 100;
    public int MaxOverflowChannels { get; set; } = 2;
}