using Microsoft.AspNetCore.Identity;
using VstepPractice.API.Data;
using VstepPractice.API.Data.Seeding.Abstracts;
using VstepPractice.API.Data.Seeding.Seeders;
using VstepPractice.API.Data.Seeding;
using VstepPractice.API.Models.Entities;

namespace VstepPractice.API.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<User, Role>(options =>
        {
            // Password settings
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;

            // User settings
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<RoleManager<Role>>();
        services.AddScoped<UserManager<User>>();
    }
}
