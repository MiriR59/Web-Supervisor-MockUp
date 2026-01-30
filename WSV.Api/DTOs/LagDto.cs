public class LagDto
{
    public int SourceId { get; set; }
    public LagState State { get; set; }
    public DateTimeOffset? LatestGenerated { get; set; }
    public DateTimeOffset? LatestDb { get; set; }
    public double? DbLag { get; set; }
}

public enum LagState
{
    Ok,
    NoLiveData,
    DbEmpty
}