using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace stockmarket_agent.Services
{
    public class KiteService
    {
        private readonly ILogger<KiteService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        private string _apiKey;
        private string _apiSecret;
        private string _accessToken;
        private string _requestToken;

        public KiteService(ILogger<KiteService> logger, HttpClient httpClient, IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
            _apiKey = _configuration["Kite:ApiKey"] ?? "qj8zy28njbiydgao";
            _apiSecret = _configuration["Kite:ApiSecret"] ?? "sc08i47zd0i7564zzqfuhhht8rg5g13q";
            _accessToken = _configuration["Kite:AccessToken"];
        }

        public async Task<string> GenerateSessionUrl(string redirectUrl)
        {
            var kiteConnectUrl = $"https://kite.zerodha.com/connect/login?v=3&api_key={_apiKey}&redirect_url={redirectUrl}";
            return kiteConnectUrl;
        }

        public async Task<bool> SetRequestToken(string requestToken)
        {
            _requestToken = requestToken;
            return await GenerateAccessToken();
        }

        private async Task<bool> GenerateAccessToken()
        {
            try
            {
                var checksum = CalculateChecksum(_requestToken, _apiSecret);
                var url = $"https://kite.trade/oms/session/token";

                var content = new StringContent(
                    $"api_key={_apiKey}&request_token={_requestToken}&checksum={checksum}",
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded"
                );

                var response = await _httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<KiteSessionResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (data?.Data?.AccessToken != null)
                    {
                        _accessToken = data.Data.AccessToken;
                        _logger.LogInformation("Successfully generated Kite access token");
                        return true;
                    }
                }

                _logger.LogError($"Failed to generate access token: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating access token: {ex.Message}");
                return false;
            }
        }

        private string CalculateChecksum(string requestToken, string apiSecret)
        {
            var raw = _apiKey + requestToken + apiSecret;
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(raw);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLower();
        }

        public async Task<KiteOrderResponse?> PlaceOrder(KiteOrderRequest orderRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(_accessToken))
                {
                    _logger.LogError("Access token not available. Please authenticate first.");
                    return null;
                }

                var url = "https://kite.trade/oms/orders/regular";
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"token {_apiKey}:{_accessToken}");

                var content = new StringContent(
                    JsonSerializer.Serialize(orderRequest),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var data = JsonSerializer.Deserialize<KiteOrderResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    _logger.LogInformation($"Order placed successfully: {data?.Data?.OrderId}");
                    return data;
                }
                else
                {
                    _logger.LogError($"Failed to place order: {response.StatusCode} - {responseContent}");
                    return JsonSerializer.Deserialize<KiteOrderResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error placing order: {ex.Message}");
                return null;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Clear();
            }
        }

        public async Task<KiteQuoteResponse?> GetQuote(string instrumentToken)
        {
            try
            {
                if (string.IsNullOrEmpty(_accessToken))
                {
                    _logger.LogError("Access token not available. Please authenticate first.");
                    return null;
                }

                var url = $"https://kite.trade/oms/quote/{instrumentToken}";
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"token {_apiKey}:{_accessToken}");

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<KiteQuoteResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return data;
                }

                _logger.LogError($"Failed to get quote: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting quote: {ex.Message}");
                return null;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Clear();
            }
        }

        public async Task<KiteOptionsChainResponse?> GetOptionsChain(string symbol)
        {
            try
            {
                if (string.IsNullOrEmpty(_accessToken))
                {
                    _logger.LogWarning("Access token not available. Returning null for options chain.");
                    return null;
                }

                // Kite API doesn't have a direct options chain endpoint
                // We need to fetch instruments and filter for options
                var url = "https://kite.trade/oms/instruments/NFO";
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"token {_apiKey}:{_accessToken}");

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var instruments = JsonSerializer.Deserialize<List<KiteInstrument>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (instruments != null)
                    {
                        // Filter for the symbol and options
                        var symbolInstruments = instruments
                            .Where(i => i.TradingSymbol.Contains(symbol) && (i.InstrumentType == "CE" || i.InstrumentType == "PE"))
                            .GroupBy(i => i.StrikePrice)
                            .Select(g => new KiteOptionsChainItem
                            {
                                StrikePrice = g.Key ?? 0,
                                CE = g.FirstOrDefault(i => i.InstrumentType == "CE"),
                                PE = g.FirstOrDefault(i => i.InstrumentType == "PE")
                            })
                            .OrderBy(i => i.StrikePrice)
                            .ToList();

                        return new KiteOptionsChainResponse
                        {
                            Data = symbolInstruments
                        };
                    }
                }

                _logger.LogError($"Failed to get instruments: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting options chain: {ex.Message}");
                return null;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Clear();
            }
        }
    }

    // Supporting classes for Kite API
    public class KiteSessionResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public KiteSessionData? Data { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    public class KiteSessionData
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("user_id")]
        public string? UserId { get; set; }
    }

    public class KiteOrderRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("exchange")]
        public string Exchange { get; set; } = "NSE";

        [System.Text.Json.Serialization.JsonPropertyName("tradingsymbol")]
        public string TradingSymbol { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("transaction_type")]
        public string TransactionType { get; set; } = string.Empty; // BUY or SELL

        [System.Text.Json.Serialization.JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("price")]
        public decimal Price { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("product")]
        public string Product { get; set; } = "CNC"; // CNC, MIS, NRML

        [System.Text.Json.Serialization.JsonPropertyName("order_type")]
        public string OrderType { get; set; } = "LIMIT"; // MARKET, LIMIT

        [System.Text.Json.Serialization.JsonPropertyName("variety")]
        public string Variety { get; set; } = "regular";
    }

    public class KiteOrderResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public KiteOrderData? Data { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string? Status { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("message")]
        public string? Message { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("error_type")]
        public string? ErrorType { get; set; }
    }

    public class KiteOrderData
    {
        [System.Text.Json.Serialization.JsonPropertyName("order_id")]
        public string? OrderId { get; set; }
    }

    public class KiteQuoteResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public Dictionary<string, KiteQuoteData>? Data { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    public class KiteQuoteData
    {
        [System.Text.Json.Serialization.JsonPropertyName("last_price")]
        public decimal? LastPrice { get; set; }
    }

    public class KiteOptionsChainResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public List<KiteOptionsChainItem>? Data { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    public class KiteOptionsChainItem
    {
        public decimal StrikePrice { get; set; }
        public KiteInstrument? CE { get; set; }
        public KiteInstrument? PE { get; set; }
    }

    public class KiteInstrument
    {
        [System.Text.Json.Serialization.JsonPropertyName("instrument_token")]
        public string? InstrumentToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("exchange_token")]
        public string? ExchangeToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("tradingsymbol")]
        public string? TradingSymbol { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string? Name { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("last_price")]
        public decimal? LastPrice { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("expiry")]
        public string? Expiry { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("strike")]
        public decimal? StrikePrice { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("tick_size")]
        public decimal? TickSize { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("lot_size")]
        public int? LotSize { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("instrument_type")]
        public string? InstrumentType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("segment")]
        public string? Segment { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("exchange")]
        public string? Exchange { get; set; }
    }
}
