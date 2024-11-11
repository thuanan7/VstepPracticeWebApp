namespace VstepPractice.API.Data.Seeding.Abstracts;

public abstract class BaseSeeder : IDataSeeder
{
    protected readonly ApplicationDbContext _context;
    protected readonly ILogger<BaseSeeder> _logger;

    protected BaseSeeder(ApplicationDbContext context, ILogger<BaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public abstract Task SeedAsync();
}
