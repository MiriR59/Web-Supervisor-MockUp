using System.ComponentModel.DataAnnotations;

namespace WSV.Api.Models;

public class AppUser
{
    public int Id { get; set; }

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
    public Boolean IsActive { get; set; } = true;
}