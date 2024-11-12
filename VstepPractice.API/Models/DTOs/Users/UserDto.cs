using VstepPractice.API.Common.Enums;

namespace VstepPractice.API.Models.DTOs.Users;

public class UserDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
}
