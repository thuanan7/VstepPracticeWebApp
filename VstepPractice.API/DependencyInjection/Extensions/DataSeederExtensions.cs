using VstepPractice.API.Data.Seeding.Abstracts;
using VstepPractice.API.Data.Seeding.Seeders;
using VstepPractice.API.Data.Seeding;

namespace VstepPractice.API.DependencyInjection.Extensions;

public static class DataSeederExtensions
{
    public static void AddSeederServices(this IServiceCollection services)
    {
        services.AddScoped<IDataSeeder, RoleSeeder>();
        //services.AddScoped<IDataSeeder, UserSeeder>();
        //services.AddScoped<IDataSeeder, ExamSeeder>();
        services.AddScoped<DataSeeder>();
    }

    public static async Task SeedDataAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAllAsync();
    }
}
