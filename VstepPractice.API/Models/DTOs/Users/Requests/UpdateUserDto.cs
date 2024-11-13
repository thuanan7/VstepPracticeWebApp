namespace VstepPractice.API.Models.DTOs.Users.Requests;

public class UpdateUserDto
{
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? Role { get; set; }
}
