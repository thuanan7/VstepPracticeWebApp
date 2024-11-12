using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VstepPractice.API.Common.Enums;
using VstepPractice.API.Common.Utils;
using VstepPractice.API.Data;
using VstepPractice.API.Models.Entities;
using VstepPractice.API.Repositories.Interfaces;

namespace VstepPractice.API.Repositories.Implementations;

public class UserRepository : RepositoryBase<User, int>, IUserRepository
{
    private readonly UserManager<User> _userManager;

    public UserRepository(
        ApplicationDbContext context,
        UserManager<User> userManager) : base(context)
    {
        _userManager = userManager;
    }

    public async Task<bool> IsEmailUniqueAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return !await FindAll()
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    public override async Task<User?> FindByIdAsync(
        int id,
        CancellationToken cancellationToken = default,
        params Expression<Func<User, object>>[] includeProperties)
    {
        // Override to include default relations
        var defaultIncludes = new List<Expression<Func<User, object>>>
        {
            u => u.CreatedExams,
            u => u.StudentAttempts
        };

        if (includeProperties != null)
            defaultIncludes.AddRange(includeProperties);

        return await base.FindByIdAsync(id, cancellationToken, defaultIncludes.ToArray());
    }

    // Additional methods specific to User entity
    public async Task<List<User>> GetTeachersAsync(
        CancellationToken cancellationToken = default)
    {
        return await FindAll(u => u.Role == UserRole.Teacher)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UpdateUserRoleAsync(
        int userId,
        UserRole newRole,
        CancellationToken cancellationToken = default)
    {
        var user = await FindByIdAsync(userId, cancellationToken);
        if (user == null)
            return false;

        user.Role = newRole;
        Update(user);

        // Update Identity roles
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, newRole.ToString());

        return true;
    }
}