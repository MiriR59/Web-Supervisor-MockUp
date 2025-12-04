using System.ComponentModel.DataAnnotations;

namespace WSV.Api.Models;

public class Source
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Boolean IsEnabled { get; set; }
    
    public string BehaviourProfile { get; set; } = string.Empty;

    public List<SourceReading> Readings { get; set; } = new();

}