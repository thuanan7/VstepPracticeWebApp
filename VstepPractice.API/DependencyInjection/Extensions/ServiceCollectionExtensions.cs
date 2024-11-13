using Betalgo.Ranul.OpenAI.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.Text;
using System.Text.Json;
using VstepPractice.API.Data;
using VstepPractice.API.Models.Entities;
using VstepPractice.API.Repositories.Implementations;
using VstepPractice.API.Repositories.Interfaces;
using VstepPractice.API.Services.AI;
using VstepPractice.API.Services.Auth;
using VstepPractice.API.Services.BackgroundServices;
using VstepPractice.API.Services.Exams;
using VstepPractice.API.Services.StudentAttempts;
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

    public static void AddDependencyInjections(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IExamService, ExamService>();
        services.AddScoped<IStudentAttemptService, StudentAttemptService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IExamRepository, ExamRepository>();
        services.AddScoped<IQuestionOptionRepository, QuestionOptionRepository>();
        services.AddScoped<IStudentAttemptRepository, StudentAttemptRepository>();
        services.AddScoped<IAnswerRepository, AnswerRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    public static void AddAuthenServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["JWT:Secret"]!)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero,
            };
        });

        services.AddScoped<IAuthService, AuthService>();
    }

    public static void AddAiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Register the background service as Singleton first
        services.AddSingleton<EssayScoringBackgroundService>();

        // 2. Register the interface implementation
        services.AddSingleton<IEssayScoringQueue>(sp =>
            sp.GetRequiredService<EssayScoringBackgroundService>());

        // 3. Register it as a hosted service
        services.AddHostedService(sp =>
            sp.GetRequiredService<EssayScoringBackgroundService>());

        // 4. OpenAI services
        services.AddOpenAIService(settings =>
        {
            settings.ApiKey = configuration["OpenAI:ApiKey"];
            settings.DefaultModelId = configuration["OpenAI:ModelName"];
        });

        services.AddOptions<OpenAiOptions>()
            .Bind(configuration.GetSection(OpenAiOptions.SectionName));

        services.AddHttpClient<IAiScoringService, OpenAiScoringService>()
        .SetHandlerLifetime(TimeSpan.FromMinutes(5))
        .ConfigureHttpClient(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(
                configuration.GetValue<int>("OpenAI:TimeoutSeconds"));
        });

        services.AddScoped<IAiScoringService, OpenAiScoringService>();
    }
}
