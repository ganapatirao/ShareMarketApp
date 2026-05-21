using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace stockmarket_agent.Services
{
    public class FinnhubService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FinnhubService> _logger;
        private readonly string _apiKey;

        public FinnhubService(
            HttpClient httpClient,
            ILogger<FinnhubService> logger,
            IOptions<FinnhubSettings> finnhubSettings)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = finnhubSettings.Value.ApiKey;
            _httpClient.BaseAddress = new Uri("https://finnhub.io/api/v1/");
        }

        public async Task<FinnhubCompanyProfile?> GetCompanyProfileAsync(string symbol)
        {
            try
            {
                // Remove .NS suffix for Finnhub API
                var cleanSymbol = symbol.Replace(".NS", "");
                var url = $"stock/profile2?symbol={cleanSymbol}&token={_apiKey}";

                _logger.LogInformation($"Fetching company profile from Finnhub for {cleanSymbol}");
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Finnhub API returned {response.StatusCode} for {cleanSymbol}");
                    return null;
                }

                var profile = await response.Content.ReadFromJsonAsync<FinnhubCompanyProfile>();
                _logger.LogInformation(
                    $"Finnhub profile for {cleanSymbol}: MarketCap={profile?.MarketCap}, PE={profile?.Pe}, Sector={profile?.Sector}, Industry={profile?.Industry}"
                );

                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching company profile from Finnhub for {symbol}: {ex.Message}");
                return null;
            }
        }
    }

    public class FinnhubSettings
    {
        public string ApiKey { get; set; } = string.Empty;
    }

    public class FinnhubCompanyProfile
    {
        [JsonPropertyName("marketCapitalization")]
        public double? MarketCap { get; set; }

        [JsonPropertyName("pe")]
        public double? Pe { get; set; }

        [JsonPropertyName("sector")]
        public string? Sector { get; set; }

        [JsonPropertyName("industry")]
        public string? Industry { get; set; }

        [JsonPropertyName("ticker")]
        public string? Ticker { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
