using Microsoft.Extensions.Logging;
using System.Text.Json;
using stockmarket_agent.Models;

namespace stockmarket_agent.Services
{
    public class StockDataFetcherService
    {
        private readonly ILogger<StockDataFetcherService> _logger;
        private readonly HttpClient _httpClient;

        public StockDataFetcherService(ILogger<StockDataFetcherService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            
            // Add User-Agent header to avoid being blocked by Yahoo Finance
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        }

        public async Task<StockData?> FetchStockDataAsync(string symbol, int retryCount = 0)
        {
            try
            {
                var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}";
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Yahoo Finance API returned {response.StatusCode} for {symbol}");
                    
                    // Implement retry with exponential backoff for rate limiting
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && retryCount < 5)
                    {
                        var delaySeconds = Math.Pow(2, retryCount) * 5; // 5, 10, 20, 40, 80 seconds
                        _logger.LogInformation($"Rate limited for {symbol}. Retrying in {delaySeconds} seconds (attempt {retryCount + 1}/5)...");
                        await Task.Delay((int)delaySeconds * 1000);
                        return await FetchStockDataAsync(symbol, retryCount + 1);
                    }
                    
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(content);
                
                var result = jsonDoc.RootElement.GetProperty("chart").GetProperty("result");
                if (result.GetArrayLength() == 0)
                {
                    _logger.LogWarning($"No data returned for {symbol}");
                    return null;
                }

                var meta = result[0].GetProperty("meta");
                var regularMarketPrice = meta.GetProperty("regularMarketPrice").GetDouble();
                var previousClose = meta.GetProperty("previousClose").GetDouble();
                
                // Optional fields - use TryGetProperty to avoid errors if missing
                var currency = meta.TryGetProperty("currency", out var currencyProp) ? currencyProp.GetString() : "INR";
                var marketState = meta.TryGetProperty("marketState", out var marketStateProp) ? marketStateProp.GetString() : "CLOSED";
                
                var stockData = new StockData
                {
                    Symbol = symbol,
                    Price = regularMarketPrice,
                    PreviousClose = previousClose,
                    PriceChange = regularMarketPrice - previousClose,
                    PriceChangePercentage = previousClose > 0 ? ((regularMarketPrice - previousClose) / previousClose) * 100 : 0,
                    LastUpdated = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Try to get additional data if available
                if (meta.TryGetProperty("fiftyTwoWeekHigh", out var week52High))
                {
                    stockData.Week52High = week52High.GetDouble();
                }
                if (meta.TryGetProperty("fiftyTwoWeekLow", out var week52Low))
                {
                    stockData.Week52Low = week52Low.GetDouble();
                }
                if (meta.TryGetProperty("regularMarketVolume", out var volume))
                {
                    stockData.Volume = volume.GetInt64();
                }
                if (meta.TryGetProperty("regularMarketDayHigh", out var high))
                {
                    stockData.High = high.GetDouble();
                }
                if (meta.TryGetProperty("regularMarketDayLow", out var low))
                {
                    stockData.Low = low.GetDouble();
                }
                if (meta.TryGetProperty("regularMarketOpen", out var open))
                {
                    stockData.Open = open.GetDouble();
                }

                _logger.LogInformation($"Successfully fetched data for {symbol}: {regularMarketPrice}");
                return stockData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching data for {symbol}: {ex.Message}");
                return null;
            }
        }
    }
}
