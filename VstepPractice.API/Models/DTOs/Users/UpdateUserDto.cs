using System.ComponentModel.DataAnnotations;
using VstepPractice.API.Common.Enums;

namespace VstepPractice.API.Models.DTOs.Users;

public class UpdateUserDto
{
    [StringLength(50)]
    public string? UserName { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public UserRole? Role { get; set; }
}
