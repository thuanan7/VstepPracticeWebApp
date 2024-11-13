using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VstepPractice.API.Common.Enums;
using VstepPractice.API.Common.Utils;
using VstepPractice.API.Models.DTOs.Users.Requests;
using VstepPractice.API.Models.DTOs.Users.Responses;
using VstepPractice.API.Models.Entities;
using VstepPractice.API.Repositories.Interfaces;

namespace VstepPractice.API.Services.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByIdAsync(id, cancellationToken,
            u => u.CreatedExams,
            u => u.StudentAttempts);

        if (user == null)
            return Result.Failure<UserDto>(Error.NotFound);

        return Result.Success(_mapper.Map<UserDto>(user));
    }

    public async Task<Result<UserDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindSingleAsync(u => u.Email == email, cancellationToken);
        if (user == null)
            return Result.Failure<UserDto>(Error.NotFound);

        return Result.Success(_mapper.Map<UserDto>(user));
    }

    public async Task<Result<PagedResult<UserDto>>> GetUsersAsync(GetUsersDto request, CancellationToken cancellationToken = default)
    {
        var usersQuery = string.IsNullOrEmpty(request.SearchTerm)
            ? _userRepository.FindAll()
            : _userRepository.FindAll(x => x.Email!.Contains(request.SearchTerm) || x.UserName!.Contains(request.SearchTerm));

        usersQuery.Include(u => u.CreatedExams)
            .Include(u => u.StudentAttempts);

        if (request.SortColumnAndOrder != null && request.SortColumnAndOrder.Count > 0)
        {
            // Biến này để theo dõi liệu điều kiện sắp xếp đầu tiên đã được áp dụng chưa
            bool firstCondition = true;

            foreach (var item in request.SortColumnAndOrder)
            {
                var keySelector = GetKeySelector(item.Key);

                if (firstCondition)
                {
                    // Điều kiện sắp xếp đầu tiên sử dụng OrderBy hoặc OrderByDescending
                    usersQuery = item.Value == SortOrder.Ascending
                        ? usersQuery.OrderBy(keySelector)
                        : usersQuery.OrderByDescending(keySelector);

                    firstCondition = false;
                }
                else
                {
                    // Các điều kiện sắp xếp tiếp theo sử dụng ThenBy hoặc ThenByDescending
                    usersQuery = item.Value == SortOrder.Ascending
                        ? ((IOrderedQueryable<User>)usersQuery).ThenBy(keySelector)
                        : ((IOrderedQueryable<User>)usersQuery).ThenByDescending(keySelector);
                }
            }
        }

        var users = await PagedResult<User>.CreateAsync(usersQuery, request.PageIndex, request.PageSize, cancellationToken);

        var result = _mapper.Map<PagedResult<UserDto>>(users);

        return Result.Success(result);
    }

    public async Task<Result<UserDto>> CreateAsync(CreateUserDto model, CancellationToken cancellationToken = default)
    {
        //if (!await _userRepository.IsEmailUniqueAsync(model.Email, cancellationToken))
        //    throw new BadRequestException("Email is already taken");

        //var user = _mapper.Map<User>(model);
        //await _userRepository.CreateAsync(user, model.Password, cancellationToken);

        //return _mapper.Map<UserDto>(user);

        return null;
    }

    public async Task<Result<UserDto>> UpdateAsync(int id, UpdateUserDto model, CancellationToken cancellationToken = default)
    {
        //var user = await _userRepository.FindByIdAsync(id, cancellationToken);
        //if (user == null)
        //    throw new NotFoundException("User not found");

        //if (model.Email != null && model.Email != user.Email)
        //{
        //    if (!await _userRepository.IsEmailUniqueAsync(model.Email, cancellationToken))
        //        throw new BadRequestException("Email is already taken");
        //}

        //_mapper.Map(model, user);

        //if (model.Role.HasValue && model.Role != user.Role)
        //{
        //    await _userRepository.UpdateUserRoleAsync(id, model.Role.Value, cancellationToken);
        //}
        //else
        //{
        //    await _userRepository.UpdateAsync(user, cancellationToken);
        //}

        //return _mapper.Map<UserDto>(user);
        return null;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        //var user = await _userRepository.FindByIdAsync(id, cancellationToken);
        //if (user == null)
        //    throw new NotFoundException("User not found");

        //await _userRepository.DeleteAsync(user, cancellationToken);
    }

    private static Expression<Func<User, object>> GetKeySelector(string? sortColumn)
        => sortColumn?.ToLower() switch
        {
            "username" => x => x.UserName!,
            "email" => x => x.Email!,
            _ => x => x.CreatedAt
        };
}