using Microsoft.AspNetCore.Identity;
using VstepPractice.API.Data;
using VstepPractice.API.Models.Entities;
using VstepPractice.API.Repositories.Implementations;
using VstepPractice.API.Repositories.Interfaces;
using VstepPractice.API.Services.Users;

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

    public static void AddUserServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
    }
}
