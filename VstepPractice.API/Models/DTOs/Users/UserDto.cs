namespace VstepPractice.API.Models.DTOs.Users;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public IList<string> Roles { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
