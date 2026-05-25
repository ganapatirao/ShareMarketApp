using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace stockmarket_agent.Services
{
    public class NSEOptionsService
    {
        private readonly ILogger<NSEOptionsService> _logger;
        private readonly HttpClient _httpClient;

        private readonly Dictionary<string, string> _symbolMapping = new Dictionary<string, string>
        {
            { "NIFTY", "NIFTY" },
            { "BANKNIFTY", "BANKNIFTY" },
            { "SENSEX", "NIFTY" }, // SENSEX uses NIFTY as fallback
            { "CRUDE", "NIFTY" } // CRUDE uses NIFTY as fallback
        };

        public NSEOptionsService(ILogger<NSEOptionsService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<(DateTime expiryDate, decimal openInterest, long volume)> GetOptionsDataAsync(string symbol)
        {
            try
            {
                var nseSymbol = _symbolMapping.TryGetValue(symbol, out var mappedSymbol) ? mappedSymbol : symbol;
                
                // NSE Options Chain API
                var url = $"https://www.nseindia.com/api/option-chain-indices?symbol={nseSymbol}";
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
                _httpClient.DefaultRequestHeaders.Add("Referer", "https://www.nseindia.com/");
                _httpClient.DefaultRequestHeaders.Add("Origin", "https://www.nseindia.com");

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"NSE API returned {response.StatusCode} for {symbol}");
                    return (GetNextWeeklyExpiry(), 0, 0);
                }

                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<NSEOptionsResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });

                if (data?.Records?.Data != null && data.Records.Data.Length > 0)
                {
                    // Find the current week expiry date
                    var currentWeekExpiry = FindCurrentWeekExpiry(data.Records.Data);
                    
                    // Calculate total OI and volume for current week
                    var (totalOI, totalVolume) = CalculateTotalOIAndVolume(data.Records.Data, currentWeekExpiry);
                    
                    return (currentWeekExpiry, totalOI, totalVolume);
                }

                return (GetNextWeeklyExpiry(), 0, 0);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching options data from NSE for {symbol}: {ex.Message}");
                return (GetNextWeeklyExpiry(), 0, 0);
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Clear();
            }
        }

        private DateTime FindCurrentWeekExpiry(NSEOptionData[] data)
        {
            var today = DateTime.Today;
            var expiryDates = data
                .Where(d => d.ExpiryDate != null)
                .Select(d => DateTime.Parse(d.ExpiryDate!))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            // Find the first expiry date that is today or in the future
            foreach (var expiry in expiryDates)
            {
                if (expiry >= today)
                {
                    return expiry;
                }
            }

            // If no future expiry found, return next Thursday
            return GetNextWeeklyExpiry();
        }

        private (decimal totalOI, long totalVolume) CalculateTotalOIAndVolume(NSEOptionData[] data, DateTime targetExpiry)
        {
            var targetExpiryStr = targetExpiry.ToString("yyyy-MM-dd");
            
            var relevantData = data.Where(d => d.ExpiryDate == targetExpiryStr);
            
            decimal totalOI = 0;
            long totalVolume = 0;

            foreach (var item in relevantData)
            {
                // Add CE data
                if (item.CE != null)
                {
                    totalOI += item.CE.OpenInterest ?? 0;
                    totalVolume += item.CE.TotalTradedVolume ?? 0;
                }

                // Add PE data
                if (item.PE != null)
                {
                    totalOI += item.PE.OpenInterest ?? 0;
                    totalVolume += item.PE.TotalTradedVolume ?? 0;
                }
            }

            return (totalOI, totalVolume);
        }

        private DateTime GetNextWeeklyExpiry()
        {
            var today = DateTime.Today;
            var weekday = (int)today.DayOfWeek;
            if (weekday == 4 && today.Hour < 15)
            {
                return today;
            }
            return today.AddDays((4 - weekday + 7) % 7);
        }

        public async Task<NSEOptionData[]?> GetOptionsChainAsync(string symbol)
        {
            try
            {
                var nseSymbol = _symbolMapping.TryGetValue(symbol, out var mappedSymbol) ? mappedSymbol : symbol;
                
                // NSE Options Chain API
                var url = $"https://www.nseindia.com/api/option-chain-indices?symbol={nseSymbol}";
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
                _httpClient.DefaultRequestHeaders.Add("Referer", "https://www.nseindia.com/");
                _httpClient.DefaultRequestHeaders.Add("Origin", "https://www.nseindia.com");

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"NSE API returned {response.StatusCode} for {symbol}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<NSEOptionsResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });

                if (data?.Records?.Data != null && data.Records.Data.Length > 0)
                {
                    // Find the current week expiry date
                    var currentWeekExpiry = FindCurrentWeekExpiry(data.Records.Data);
                    var targetExpiryStr = currentWeekExpiry.ToString("yyyy-MM-dd");
                    
                    // Filter data for current week expiry and return
                    var optionsChain = data.Records.Data
                        .Where(d => d.ExpiryDate == targetExpiryStr)
                        .OrderBy(d => d.StrikePrice)
                        .ToArray();
                    
                    return optionsChain;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching options chain from NSE for {symbol}: {ex.Message}");
                return null;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Clear();
            }
        }
    }

    // Supporting classes for NSE API response
    public class NSEOptionsResponse
    {
        [JsonPropertyName("records")]
        public NSERecords? Records { get; set; }
    }

    public class NSERecords
    {
        [JsonPropertyName("data")]
        public NSEOptionData[]? Data { get; set; }
    }

    public class NSEOptionData
    {
        [JsonPropertyName("expiryDate")]
        public string? ExpiryDate { get; set; }

        [JsonPropertyName("strikePrice")]
        public decimal? StrikePrice { get; set; }

        [JsonPropertyName("CE")]
        public NSEOptionDetails? CE { get; set; }

        [JsonPropertyName("PE")]
        public NSEOptionDetails? PE { get; set; }
    }

    public class NSEOptionDetails
    {
        [JsonPropertyName("strikePrice")]
        public decimal? StrikePrice { get; set; }

        [JsonPropertyName("lastPrice")]
        public decimal? LastPrice { get; set; }

        [JsonPropertyName("openInterest")]
        public decimal? OpenInterest { get; set; }

        [JsonPropertyName("totalTradedVolume")]
        public long? TotalTradedVolume { get; set; }

        [JsonPropertyName("impliedVolatility")]
        public decimal? ImpliedVolatility { get; set; }

        [JsonPropertyName("changeinOpenInterest")]
        public decimal? ChangeInOpenInterest { get; set; }

        [JsonPropertyName("pChangeinOpenInterest")]
        public decimal? PChangeInOpenInterest { get; set; }
    }
}
