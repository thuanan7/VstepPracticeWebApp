using Betalgo.Ranul.OpenAI.Extensions;
using VstepPractice.API.Services.AI;

namespace VstepPractice.API.DependencyInjection.Extensions;

public static class OpenAiServiceExtensions
{
    public static IServiceCollection AddOpenAiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var apiKey = configuration["OPENAI:API_KEY"];

        // Configure OpenAiOptions combining both sources
        services.Configure<OpenAiOptions>(options =>
        {
            // Load non-sensitive settings from appsettings.json
            configuration.GetSection(OpenAiOptions.SectionName).Bind(options);

            // Override API key from environment variable
            options.ApiKey = apiKey;
        });

        // Configure Betalgo OpenAI client
        services.AddOpenAIService(settings =>
        {
            settings.ApiKey = apiKey;
            settings.DefaultModelId = configuration[$"{OpenAiOptions.SectionName}:ModelName"];
        });

        // Register our scoring service
        services.AddScoped<IAiScoringService, OpenAiScoringService>();

        return services;
    }
}
