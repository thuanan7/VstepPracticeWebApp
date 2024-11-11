using VstepPractice.API.Data.Seeding.Abstracts;

namespace VstepPractice.API.Data.Seeding;

public class DataSeeder
{
    private readonly IEnumerable<IDataSeeder> _seeders;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(IEnumerable<IDataSeeder> seeders, ILogger<DataSeeder> logger)
    {
        _seeders = seeders;
        _logger = logger;
    }

    public async Task SeedAllAsync()
    {
        try
        {
            foreach (var seeder in _seeders)
            {
                _logger.LogInformation("Running seeder: {SeederType}", seeder.GetType().Name);
                await seeder.SeedAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}