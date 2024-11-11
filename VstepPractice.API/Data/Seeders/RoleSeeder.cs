using Microsoft.AspNetCore.Identity;
using VstepPractice.API.Common.Constant;
using VstepPractice.API.Common.Enums;
using VstepPractice.API.Models.Entities;

namespace VstepPractice.API.Data.Seeders;

public class RoleSeeder : IDataSeeder
{
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<RoleSeeder> _logger;

    public RoleSeeder(
        RoleManager<Role> roleManager,
        UserManager<User> userManager,
        ILogger<RoleSeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Seed Roles
            foreach (var roleName in Roles.All)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var role = new Role(roleName);
                    var result = await _roleManager.CreateAsync(role);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Created new role: {Role}", roleName);
                    }
                    else
                    {
                        _logger.LogError("Failed to create role {Role}", roleName);
                    }
                }
            }

            // Seed Admin User
            var adminEmail = "admin@example.com";
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = "admin",
                    Email = adminEmail,
                    Role = UserRole.Admin
                };

                var result = await _userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, Roles.Admin);
                    _logger.LogInformation("Created admin user: {Email}", adminEmail);
                }
                else
                {
                    _logger.LogError("Failed to create admin user");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while seeding data");
            throw;
        }
    }
}
