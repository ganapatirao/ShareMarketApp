using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace stockmarket_agent.Services
{
    public class YahooFinanceOptionsService
    {
        private readonly ILogger<YahooFinanceOptionsService> _logger;
        private readonly HttpClient _httpClient;

        private readonly Dictionary<string, string> _symbolMapping = new Dictionary<string, string>
        {
            { "NIFTY", "^NSEI" },
            { "BANKNIFTY", "^NSEBANK" },
            { "SENSEX", "^BSESN" },
            { "CRUDE", "CL=F" }
        };

        public YahooFinanceOptionsService(ILogger<YahooFinanceOptionsService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<List<YahooOptionsChainItem>?> GetOptionsChainAsync(string symbol)
        {
            try
            {
                var yahooSymbol = _symbolMapping.TryGetValue(symbol, out var mappedSymbol) ? mappedSymbol : symbol;
                
                // Yahoo Finance doesn't have a direct options chain API for Indian indices
                // Return mock data based on the symbol's current price
                var mockData = GenerateMockOptionsChain(symbol);
                return mockData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating mock options chain for {symbol}: {ex.Message}");
                return null;
            }
        }

        private List<YahooOptionsChainItem> GenerateMockOptionsChain(string symbol)
        {
            var optionsChain = new List<YahooOptionsChainItem>();
            
            // Generate mock strike prices based on symbol
            decimal basePrice = symbol switch
            {
                "NIFTY" => 22500,
                "BANKNIFTY" => 48000,
                "SENSEX" => 74000,
                "CRUDE" => 90,
                _ => 1000
            };

            // Generate strike prices around the base price
            var strikes = new List<decimal>();
            for (int i = -10; i <= 10; i++)
            {
                var increment = symbol switch
                {
                    "NIFTY" => 50,
                    "BANKNIFTY" => 100,
                    "SENSEX" => 200,
                    "CRUDE" => 1,
                    _ => 10
                };
                strikes.Add(basePrice + (i * increment));
            }

            foreach (var strike in strikes.OrderBy(s => s))
            {
                var volume = new Random().Next(100, 1000);
                var call = new YahooOptionData
                {
                    Strike = strike,
                    LastPrice = Math.Max(0, (basePrice - strike) * 0.5m + 10),
                    OpenInterest = new Random().Next(1000, 10000),
                    Volume = volume,
                    TotalTradedVolume = volume,
                    ImpliedVolatility = new Random().Next(15, 30) / 100m,
                    ChangeInOpenInterest = new Random().Next(-500, 500)
                };

                var putVolume = new Random().Next(100, 1000);
                var put = new YahooOptionData
                {
                    Strike = strike,
                    LastPrice = Math.Max(0, (strike - basePrice) * 0.5m + 10),
                    OpenInterest = new Random().Next(1000, 10000),
                    Volume = putVolume,
                    TotalTradedVolume = putVolume,
                    ImpliedVolatility = new Random().Next(15, 30) / 100m,
                    ChangeInOpenInterest = new Random().Next(-500, 500)
                };

                optionsChain.Add(new YahooOptionsChainItem
                {
                    StrikePrice = strike,
                    CE = call,
                    PE = put
                });
            }

            return optionsChain;
        }
    }

    // Supporting classes for Yahoo Finance API response
    public class YahooOptionsResponse
    {
        [JsonPropertyName("optionChain")]
        public YahooOptionChain? OptionChain { get; set; }
    }

    public class YahooOptionChain
    {
        [JsonPropertyName("result")]
        public YahooOptionResult[]? Result { get; set; }
    }

    public class YahooOptionResult
    {
        [JsonPropertyName("options")]
        public YahooOptions? Options { get; set; }
    }

    public class YahooOptions
    {
        [JsonPropertyName("calls")]
        public YahooOptionData[]? Calls { get; set; }

        [JsonPropertyName("puts")]
        public YahooOptionData[]? Puts { get; set; }
    }

    public class YahooOptionData
    {
        [JsonPropertyName("strike")]
        public decimal? Strike { get; set; }

        [JsonPropertyName("lastPrice")]
        public decimal? LastPrice { get; set; }

        [JsonPropertyName("openInterest")]
        public decimal? OpenInterest { get; set; }

        [JsonPropertyName("volume")]
        public long? Volume { get; set; }

        [JsonPropertyName("totalTradedVolume")]
        public long? TotalTradedVolume { get; set; }

        [JsonPropertyName("impliedVolatility")]
        public decimal? ImpliedVolatility { get; set; }

        [JsonPropertyName("changeinOpenInterest")]
        public decimal? ChangeInOpenInterest { get; set; }
    }

    public class YahooOptionsChainItem
    {
        public decimal StrikePrice { get; set; }
        public YahooOptionData? CE { get; set; }
        public YahooOptionData? PE { get; set; }
    }
}
