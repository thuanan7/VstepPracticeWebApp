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
            foreach (string roleName in RoleConstants.All)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new Role(roleName));
                    _logger.LogInformation("Created role: {RoleName}", roleName);
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
