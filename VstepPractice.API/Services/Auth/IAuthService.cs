using VstepPractice.API.Common.Utils;
using VstepPractice.API.Models.DTOs.Auth;

namespace VstepPractice.API.Services.Auth;

public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> LogoutAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
