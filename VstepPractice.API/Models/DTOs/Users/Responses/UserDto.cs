namespace VstepPractice.API.Models.DTOs.Users.Responses;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public IList<string> Roles { get; set; } = default!;
}
