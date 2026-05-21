using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace stockmarket_agent.Services
{
    public class DailyStockDataUpdateService : BackgroundService
    {
        private readonly StockDataFetcherService _stockDataFetcherService;
        private readonly ILogger<DailyStockDataUpdateService> _logger;
        private readonly TimeSpan _updateInterval = TimeSpan.FromHours(24); // Run every 24 hours

        public DailyStockDataUpdateService(
            StockDataFetcherService stockDataFetcherService,
            ILogger<DailyStockDataUpdateService> logger)
        {
            _stockDataFetcherService = stockDataFetcherService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daily Stock Data Update Service started.");

            // Run immediately on startup
            await RunUpdateAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // calculate next run time (next day at 9:30 AM IST, which is 4:00 AM UTC)
                    var now = DateTime.UtcNow;
                    var nextRun = new DateTime(now.Year, now.Month, now.Day, 4, 0, 0, DateTimeKind.Utc);

                    if (now > nextRun)
                    {
                        nextRun = nextRun.AddDays(1);
                    }

                    var delay = nextRun - now;
                    _logger.LogInformation($"Next scheduled update at {nextRun:yyyy-MM-dd HH:mm:ss} UTC (in {delay.TotalHours:f1} hours)");

                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await RunUpdateAsync();
                    }
                }
                catch (TaskCanceledException)
                {
                    // Graceful shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in daily update service: {ex.Message}");
                    // Wait for a shorter interval before retrying
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }
            }

            _logger.LogInformation("Daily Stock Data Update Service stopped.");
        }

        private async Task RunUpdateAsync()
        {
            _logger.LogInformation("Starting daily stock data update...");
            try
            {
                await _stockDataFetcherService.FetchAndStoreStockDataAsync();
                _logger.LogInformation("Daily stock data update completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Daily stock data update Failed: {ex.Message}");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Daily Stock Data Update Service is stopping.");
            await base.StopAsync(cancellationToken);
        }
    }
}
