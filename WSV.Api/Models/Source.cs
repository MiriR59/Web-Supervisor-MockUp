using System.ComponentModel.DataAnnotations;

namespace WSV.Api.Models;

public class Source
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Boolean IsEnabled { get; set; }
    
    public Boolean IsPublic { get; set; } = false;

    public BehaviourProfile Behaviour { get; set; }

    public List<SourceReading> Readings { get; set; } = new();

}