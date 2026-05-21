using Microsoft.EntityFrameworkCore;
using stockmarket_agent.Data;
using stockmarket_agent.Services;

namespace stockmarket_agent.Services
{
    public class BackgroundSeedingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundSeedingService> _logger;

        public BackgroundSeedingService(IServiceProvider serviceProvider, ILogger<BackgroundSeedingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background seeding service started.");

            // Wait a bit to ensure the web server is fully started
            await Task.Delay(5000, stoppingToken);

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<StockMarketDbContext>();
                dbContext.Database.EnsureCreated();
                
                var companies = DbSeeder.GetInitialCompanies();
                if (!dbContext.Companies.Any())
                {
                    dbContext.Companies.AddRange(companies);
                    dbContext.SaveChanges();
                    _logger.LogInformation("Seeded initial companies to in-memory database.");
                }

                // Seed MongoDB with stock data in the background
                var mongoStockService = scope.ServiceProvider.GetRequiredService<MongoStockService>();
                _logger.LogInformation("Starting MongoDB seeding in background...");
                await mongoStockService.SeedInitialDataAsync();
                _logger.LogInformation("MongoDB seeding completed.");
            }

            _logger.LogInformation("Background seeding service completed.");
        }
    }
}
