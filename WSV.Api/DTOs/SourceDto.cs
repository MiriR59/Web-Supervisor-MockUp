public class SourceDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Boolean IsEnabled { get; set; }
    public string BehaviourProfile { get; set; } = string.Empty;
}