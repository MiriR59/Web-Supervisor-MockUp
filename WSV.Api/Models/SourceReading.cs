using System.ComponentModel.DataAnnotations;

namespace WSV.Api.Models;

public class SourceReading
{
    public int Id { get; set; }

    public int SourceId { get; set; }
    public Source Source { get; set; } = null!;

    public DateTimeOffset Timestamp { get; set;}

    public string Status { get; set; } = string.Empty;

    public int RPM { get; set; }
    public int Power { get; set; }
    public double Temperature { get; set; }

}