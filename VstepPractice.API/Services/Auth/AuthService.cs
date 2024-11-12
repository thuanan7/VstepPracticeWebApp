using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VstepPractice.API.Common.Constant;
using VstepPractice.API.Common.Utils;
using VstepPractice.API.Models.DTOs.Auth;
using VstepPractice.API.Models.DTOs.Users;
using VstepPractice.API.Models.Entities;

namespace VstepPractice.API.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public AuthService(
        UserManager<User> userManager,
        IConfiguration configuration,
        IMapper mapper)
    {
        _userManager = userManager;
        _configuration = configuration;
        _mapper = mapper;
    }

    public async Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Result.Failure<AuthResponse>(new Error(
                "Auth.InvalidCredentials",
                "Invalid email or password."));
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return Result.Failure<AuthResponse>(new Error(
                "Auth.InvalidCredentials",
                "Invalid email or password."));
        }

        var token = await GenerateJwtToken(user);
        var response = new AuthResponse
        {
            Token = token,
            User = _mapper.Map<UserDto>(user),
            Expiration = DateTime.UtcNow.AddDays(7)
        };

        return Result.Success(response);
    }

    public async Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var userExists = await _userManager.FindByEmailAsync(request.Email);
        if (userExists != null)
        {
            return Result.Failure<AuthResponse>(new Error(
                "Auth.EmailTaken",
                "Email is already taken."));
        }

        var user = new User
        {
            Email = request.Email,
            UserName = request.Username,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return Result.Failure<AuthResponse>(new Error(
                "Auth.RegistrationFailed",
                "User registration failed."));
        }

        // Assign default role
        await _userManager.AddToRoleAsync(user, RoleConstants.Student);

        var token = await GenerateJwtToken(user);
        var response = new AuthResponse
        {
            Token = token,
            User = _mapper.Map<UserDto>(user),
            Expiration = DateTime.UtcNow.AddDays(7)
        };

        return Result.Success(response);
    }

    public async Task<Result> LogoutAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        // Implement any logout logic here (e.g., invalidating refresh tokens)
        return Result.Success();
    }

    private async Task<string> GenerateJwtToken(User user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(CustomClaimTypes.UserId, user.Id.ToString()),
            new(CustomClaimTypes.Email, user.Email!),
            new(CustomClaimTypes.UserName, user.UserName!),
        };

        claims.AddRange(userRoles.Select(role => new Claim(CustomClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? throw new InvalidOperationException()));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.UtcNow.AddDays(7),
            claims: claims,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}