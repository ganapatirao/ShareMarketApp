using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using stockmarket_agent.Models;

namespace stockmarket_agent.Services
{
    public class OptionsDataService
    {
        private readonly ILogger<OptionsDataService> _logger;
        private readonly HttpClient _httpClient;

        private readonly Dictionary<string, string> _symbolMapping = new Dictionary<string, string>
        {
            { "NIFTY", "^NSEI" },
            { "BANKNIFTY", "^NSEBANK" },
            { "SENSEX", "^BSESN" },
            { "CRUDE", "CL=F" }
        };

        public OptionsDataService(ILogger<OptionsDataService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<List<OptionsData>> GetOptionsDataAsync()
        {
            try
            {
                var optionsData = new List<OptionsData>();
                var symbols = new List<string> { "NIFTY", "BANKNIFTY", "SENSEX", "CRUDE" };

                foreach (var symbol in symbols)
                {
                    try
                    {
                        var data = await FetchIndexDataFromYahooAsync(symbol);
                        if (data != null)
                        {
                            optionsData.Add(data);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error fetching data for {symbol}: {ex.Message}");
                    }
                }
                return optionsData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching options data: {ex.Message}");
                return new List<OptionsData>();
            }
        }

        public async Task<List<OptionsData>> GetOptionsDataBySymbolAsync(string symbol)
        {
            try
            {
                var data = await FetchIndexDataFromYahooAsync(symbol);
                if (data != null)
                {
                    return new List<OptionsData> { data };
                }
                return new List<OptionsData>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching options data for {symbol}: {ex.Message}");
                return new List<OptionsData>();
            }
        }

        private async Task<OptionsData?> FetchIndexDataFromYahooAsync(string symbol)
        {
            try
            {
                var yahooSymbol = _symbolMapping.TryGetValue(symbol, out var mappedSymbol) ? mappedSymbol : symbol;
                var chartUrl = $"https://query1.finance.yahoo.com/v8/finance/chart/{yahooSymbol}?interval=1d&range=5d&includePrePost=false";

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                var response = await _httpClient.GetAsync(chartUrl);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Yahoo Finance API returned {response.StatusCode} for {symbol}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<YahooFinanceResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });

                if (data?.Chart?.Result?.Length > 0)
                {
                    var result = data.Chart.Result[0];
                    var meta = result.Meta;
                    var quote = result.Indicators.Quote[0];

                    if (meta != null)
                    {
                        var regularPrice = (decimal)(meta.RegularMarketPrice ?? 0);
                        var previousClose = (decimal)(meta.RegularMarketPreviousClose ?? 0);
                        var change = regularPrice - previousClose;
                        var changePercentApp = previousClose == 0 ? 0 : (change / previousClose) * 100;

                        var (s1, s2, s3, s4, s5, r1, r2, r3, r4, r5) = CalculateSupportResistance(regularPrice, quote);

                        var trend = changePercentApp >= 0 ? "Bullish" : "Bearish";
                        var marketCap = GetMarketAnalysis(symbol);

                        return new OptionsData
                        {
                            Symbol = symbol,
                            RegularPrice = regularPrice,
                            PreviousClose = previousClose,
                            Change = change,
                            ChangePercentApp = changePercentApp,
                            Trend = trend,
                            MarketCap = marketCap,
                            Support1 = s1,
                            Support2 = s2,
                            Support3 = s3,
                            Support4 = s4,
                            Support5 = s5,
                            Resistance1 = r1,
                            Resistance2 = r2,
                            Resistance3 = r3,
                            Resistance4 = r4,
                            Resistance5 = r5,
                            OpenInterest = 0,
                            Volume = (long)(quote?.Volume?.FirstOrDefault() ?? 0),
                            LastUpdated = DateTime.UtcNow
                        };
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching data for {symbol} from Yahoo Finance: {ex.Message}");
                return null;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Clear();
            }
        }

        private (double s1, double s2, double s3, double s4, double s5,
                 double r1, double r2, double r3, double r4, double r5)
        CalculateSupportResistance(decimal spotPrice, YahooQuote? quote)
        {
            var highValues = quote?.High?.Where(h => h > 0).Select(h => (double?)h);
            var lowValues = quote?.Low?.Where(l => l > 0).Select(l => (double?)l);

            var high = highValues?.Any() == true ? highValues.Max()!.Value : (double)spotPrice * 1.05;
            var low = lowValues?.Any() == true ? lowValues.Min()!.Value : (double)spotPrice * 0.95;
            var close = (double)spotPrice;

            var pivot = (high + low + close) / 3;

            var s1 = (2 * pivot) - high;
            var s2 = pivot - (high - low);
            var s3 = low - 2 * (high - pivot);
            var s4 = low - 3 * (high - pivot);
            var s5 = low - 4 * (high - pivot);

            var r1 = (2 * pivot) - low;
            var r2 = pivot + (high - low);
            var r3 = high + 2 * (pivot - low);
            var r4 = high + 3 * (pivot - low);
            var r5 = high + 4 * (pivot - low);

            return (s1, s2, s3, s4, s5, r1, r2, r3, r4, r5);
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

        private string GetIndexName(string symbol)
        {
            return symbol switch
            {
                "NIFTY" => "NIFTY 50",
                "BANKNIFTY" => "BANK NIFTY",
                "SENSEX" => "SENSEX",
                "CRUDE" => "CRUDE OIL",
                _ => symbol
            };
        }

        private string GetMarketAnalysis(string symbol)
        {
            return symbol switch
            {
                "NIFTY" => "Large",
                "BANKNIFTY" => "Large",
                "SENSEX" => "Large",
                "CRUDE" => "Commodity",
                _ => "General"
            };
        }
    }

    // Supporting classes for Yahoo Finance API response
    public class YahooFinanceResponse
    {
        [JsonPropertyName("chart")]
        public YahooChart? Chart { get; set; }
    }

    public class YahooChart
    {
        [JsonPropertyName("result")]
        public YahooResult[]? Result { get; set; }
    }

    public class YahooResult
    {
        [JsonPropertyName("meta")]
        public YahooMeta? Meta { get; set; }

        [JsonPropertyName("indicators")]
        public YahooIndicators? Indicators { get; set; }
    }

    public class YahooMeta
    {
        [JsonPropertyName("regularMarketPrice")]
        public decimal? RegularMarketPrice { get; set; }

        [JsonPropertyName("regularMarketPreviousClose")]
        public decimal? RegularMarketPreviousClose { get; set; }
    }

    public class YahooIndicators
    {
        [JsonPropertyName("quote")]
        public YahooQuote[]? Quote { get; set; }
    }

    public class YahooQuote
    {
        [JsonPropertyName("high")]
        public decimal[]? High { get; set; }

        [JsonPropertyName("low")]
        public decimal[]? Low { get; set; }

        [JsonPropertyName("volume")]
        public long[]? Volume { get; set; }
    }
}
