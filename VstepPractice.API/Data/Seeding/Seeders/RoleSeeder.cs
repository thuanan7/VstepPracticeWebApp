using Microsoft.AspNetCore.Identity;
using VstepPractice.API.Common.Constant;
using VstepPractice.API.Data.Seeding.Abstracts;
using VstepPractice.API.Models.Entities;

namespace VstepPractice.API.Data.Seeding.Seeders;

public class RoleSeeder : BaseIdentitySeeder
{
    public RoleSeeder(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        ILogger<RoleSeeder> logger)
        : base(userManager, roleManager, logger)
    {
    }

    public override async Task SeedAsync()
    {
        try
        {
            // Seed Roles
            foreach (var roleName in RoleConstants.All)
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding roles");
            throw;
        }
    }
}
