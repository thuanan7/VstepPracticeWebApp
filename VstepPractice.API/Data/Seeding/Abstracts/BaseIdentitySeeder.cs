using Microsoft.AspNetCore.Identity;
using VstepPractice.API.Models.Entities;

namespace VstepPractice.API.Data.Seeding.Abstracts;

public abstract class BaseIdentitySeeder : IDataSeeder
{
    protected readonly UserManager<User> _userManager;
    protected readonly RoleManager<Role> _roleManager;
    protected readonly ILogger<BaseIdentitySeeder> _logger;

    protected BaseIdentitySeeder(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        ILogger<BaseIdentitySeeder> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public abstract Task SeedAsync();
}