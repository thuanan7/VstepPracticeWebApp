using VstepPractice.API.Common.Utils;
using VstepPractice.API.Models.DTOs.Users;

namespace VstepPractice.API.Services.Users;

public interface IUserService
{
    Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<UserDto>>> GetUsersAsync(GetUsersDto filter, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> CreateAsync(CreateUserDto model, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> UpdateAsync(int id, UpdateUserDto model, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
