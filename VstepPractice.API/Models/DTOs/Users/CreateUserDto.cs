using System.ComponentModel.DataAnnotations;
using VstepPractice.API.Common.Enums;

namespace VstepPractice.API.Models.DTOs.Users;

public class CreateUserDto
{
    [Required]
    [StringLength(50)]
    public string UserName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = null!;

    public UserRole Role { get; set; } = UserRole.User;
}
