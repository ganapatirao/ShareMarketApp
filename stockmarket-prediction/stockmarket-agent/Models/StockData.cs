using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace stockmarket_agent.Models
{
    public class StockData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonIgnore]
        public string Id { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public double Price { get; set; }
        public double Week52High { get; set; }
        public double Week52Low { get; set; }
        public double DiscountFromHigh { get; set; }
        public string MarketCapCategory { get; set; } = string.Empty;
        public double BuyPrice { get; set; }
        public double TargetPrice { get; set; }
        public string Trend { get; set; } = string.Empty;
        public long Volume { get; set; }
        public string Sector { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public double PriceChangePercentage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Additional properties for stock data fetching and analysis
        public double? PreviousClose { get; set; }
        public double? PriceChange { get; set; }
        public double? High { get; set; }
        public double? Low { get; set; }
        public double? Open { get; set; }
        public double? Close { get; set; }
        public DateTime? LastUpdated { get; set; }

        // Technical analysis properties
        public double? RSI { get; set; }
        public double? MACD { get; set; }
        public double? VolumeSMA { get; set; }
        public double? SupportLevel { get; set; }
        public double? ResistanceLevel { get; set; }
        public double? Volatility { get; set; }
        public double? Momentum { get; set; }
        public DateTime? AnalysisDate { get; set; }

        // Historical data for accurate technical analysis
        [JsonIgnore]
        public List<double>? HistoricalClosePrices { get; set; } = new List<double>();
    }

    public class TechnicalAnalysis
    {
        public string Symbol { get; set; } = string.Empty;
        public double RSI { get; set; }
        public double MACD { get; set; }
        public double Signal { get; set; }
        public double SMA20 { get; set; }
        public double SMA50 { get; set; }
        public double SMA200 { get; set; }
        public double Support { get; set; }
        public double Resistance { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }

    public class FundamentalAnalysis
    {
        public string Symbol { get; set; } = string.Empty;
        public double PERatio { get; set; }
        public double PBRatio { get; set; }
        public double DividendYield { get; set; }
        public double ROE { get; set; }
        public double DebtToEquity { get; set; }
        public double CurrentRatio { get; set; }
        public double EPS { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }

    public class StockComparison
    {
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public double Price { get; set; }
        public double PE { get; set; }
        public double MarketCap { get; set; }
        public double Volume { get; set; }
        public double GrowthScore { get; set; }
        public double ValueScore { get; set; }
        public double OverallScore { get; set; }
    }

    public class SectorAnalysis
    {
        public string Sector { get; set; } = string.Empty;
        public int TotalCompanies { get; set; }
        public double AvgPE { get; set; }
        public double AvgMarketCap { get; set; }
        public double AvgVolume { get; set; }
        public double BullishCount { get; set; }
        public double BearishCount { get; set; }
        public double SidewaysCount { get; set; }
        public string Trend { get; set; } = string.Empty;
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
        public int Count { get; set; }
        public int TotalCount { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}
