using VstepPractice.API.Models.DTOs.Users.Responses;

namespace VstepPractice.API.Models.DTOs.Auth.Responses;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public UserDto User { get; set; } = default!;
}
