using GoWheels.Data.Initialization;

namespace GoWheels.Services.Startup;

public class DbInitializerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DbInitializerService> _logger;

    public DbInitializerService(IServiceProvider serviceProvider, ILogger<DbInitializerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Database Initializer Service is starting.");

        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                // We keep migration synchronous (before seeding) but it's inside ExecuteAsync 
                // so it won't block the main app from starting if we register it as a HostedService.
                // However, if the app starts serving requests before migrations are done, it might crash.
                // The user asked to serve website and initialize in parallel.
                
                _logger.LogInformation("Seeding database...");
                await DbInitializer.SeedAsync(scope.ServiceProvider);
                _logger.LogInformation("Database seeding completed.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}
