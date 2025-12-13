public class ReadingDTO
{
    public int SourceId { get; set; }

    public DateTime Timestamp { get; set; }
    public string Status { get; set ;} = string.Empty;

    public int RPM { get; set; }
    public int Power { get; set; }
    public double Temperature { get; set; }
}