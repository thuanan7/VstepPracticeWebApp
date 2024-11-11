using Microsoft.AspNetCore.Identity;
using VstepPractice.API.Common.Constant;
using VstepPractice.API.Common.Enums;
using VstepPractice.API.Data.Seeding.Abstracts;
using VstepPractice.API.Models.Entities;

namespace VstepPractice.API.Data.Seeding.Seeders;

public class UserSeeder : BaseIdentitySeeder
{
    private readonly Dictionary<string, (string Password, UserRole Role)> _usersToSeed = new()
    {
        { "admin@vstep.com", ("Admin@123", UserRole.Admin) },
        { "teacher1@vstep.com", ("Teacher@123", UserRole.Teacher) },
        { "teacher2@vstep.com", ("Teacher@123", UserRole.Teacher) },
        { "student1@vstep.com", ("Student@123", UserRole.Student) },
        { "student2@vstep.com", ("Student@123", UserRole.Student) }
    };

    public UserSeeder(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        ILogger<UserSeeder> logger)
        : base(userManager, roleManager, logger)
    {
    }

    public override async Task SeedAsync()
    {
        try
        {
            foreach (var (email, (password, role)) in _usersToSeed)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = email,
                        Email = email,
                        Role = role,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, role.ToString());
                        _logger.LogInformation("Created user: {Email} with role: {Role}", email, role);
                    }
                    else
                    {
                        _logger.LogError("Failed to create user: {Email}", email);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding users");
            throw;
        }
    }
}
