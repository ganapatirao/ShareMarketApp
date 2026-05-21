    using stockmarket_agent.Models;
using Microsoft.Extensions.Logging;

namespace stockmarket_agent.Services
{
    public class StockAnalysisService
    {
        private readonly ILogger<StockAnalysisService> _logger;

        public StockAnalysisService(ILogger<StockAnalysisService> logger)
        {
            _logger = logger;
        }

        public StockData AnalyzeStock(StockData stock)
        {
            if (stock == null) return null;

            stock.Trend = CalculateTrend(stock.PriceChangePercentage);
            stock.MarketCapCategory = CalculateMarketCapCategoryByPrice(stock.Price);
            // Sector is now fetched from company data during seeding, not calculated here

            stock.RSI = (double?)CalculateRSI(stock.PriceChangePercentage, stock.HistoricalClosePrices);
            stock.MACD = (double?)CalculateMACD(stock.PriceChangePercentage);
            stock.VolumeSMA = stock.Volume;

            stock.SupportLevel = stock.Low.HasValue ? (double?)(stock.Low.Value * 0.95) : null;
            stock.ResistanceLevel = stock.High.HasValue ? (double?)(stock.High.Value * 1.05) : null;

            stock.Volatility = (double?)CalculateVolatility(stock.High, stock.Low, stock.Close);
            stock.Momentum = (double?)CalculateMomentum(stock.PriceChangePercentage);

            if (stock.RSI.HasValue && stock.SupportLevel.HasValue && stock.ResistanceLevel.HasValue)
            {
                stock.BuyPrice = CalculateBuyPrice(stock);
                stock.TargetPrice = CalculateTargetPrice(stock);
            }
            else
            {
                stock.BuyPrice = 0;
                stock.TargetPrice = 0;
            }

            stock.AnalysisDate = DateTime.UtcNow;
            return stock;
        }

        private string CalculateTrend(double priceChangePercentage)
        {
            if (priceChangePercentage > 2) return "Bullish";
            else if (priceChangePercentage < -2) return "Bearish";
            else return "Sideways";
        }

        private string CalculateMarketCapCategoryByPrice(double price)
        {
            if (price > 2000) return "Large Cap";
            else if (price > 500) return "Mid Cap";
            else return "Small Cap";
        }

        private double CalculateRSI(double priceChangePercentage, List<double>? historicalClosePrices = null)
        {
            if (historicalClosePrices != null && historicalClosePrices.Count >= 14)
            {
                var gains = new List<double>();
                var losses = new List<double>();

                for (int i = 1; i < historicalClosePrices.Count; i++)
                {
                    var change = historicalClosePrices[i] - historicalClosePrices[i - 1];
                    if (change > 0) { gains.Add(change); losses.Add(0); }
                    else { gains.Add(0); losses.Add(Math.Abs(change)); }
                }

                var avgGain = gains.TakeLast(14).Average();
                var avgLoss = losses.TakeLast(14).Average();

                if (avgLoss == 0) return 100;

                var rs = avgGain / avgLoss;
                var rsi = 100 - (100 / (1 + rs));
                return Math.Round(rsi, 2);
            }

            // Improved fallback: Use price change percentage to estimate RSI
            // RSI ranges from 0-100, with 50 being neutral
            // Positive price changes push RSI above 50, negative push it below 50
            var rsiEstimate = 50 + (priceChangePercentage * 2); // Scale the percentage
            
            // Clamp RSI between 0 and 100
            rsiEstimate = Math.Max(0, Math.Min(100, rsiEstimate));
            
            return Math.Round(rsiEstimate, 2);
        }

        private double CalculateMACD(double priceChangePercentage)
        {
            return Math.Round(priceChangePercentage / 2, 2);
        }

        private double CalculateVolatility(double? high, double? low, double? close)
        {
            if (!high.HasValue || !low.HasValue || !close.HasValue) return 0;
            var range = high.Value - low.Value;
            return Math.Round((range / close.Value) * 100, 2);
        }

        private double CalculateMomentum(double priceChangePercentage)
        {
            return Math.Round(priceChangePercentage, 2);
        }

        private double CalculateBuyPrice(StockData stock)
        {
            if (stock == null || stock.Price <= 0) return 0;

            double price = stock.Price;
            double rsi = stock.RSI ?? 50;
            double macd = stock.MACD ?? 0;
            double volatility = stock.Volatility ?? 0;
            double? support = stock.SupportLevel;
            double discountFromHigh = stock.DiscountFromHigh;
            double week52High = stock.Week52High;
            double week52Low = stock.Week52Low;

            // Calculate Fibonacci retracement levels for buy price
            double fib38_2 = price - (price - week52Low) * 0.382;
            double fib50 = price - (price - week52Low) * 0.5;
            double fib61_8 = price - (price - week52Low) * 0.618;

            // Determine best Fibonacci level based on RSI and MACD
            double fibBuyPrice = fib50; // Default to 50% retracement
            
            // If RSI is oversold (<30), use 38.2% retracement (less discount)
            if (rsi < 30) fibBuyPrice = fib38_2;
            // If RSI is neutral (30-70), use 50% retracement
            else if (rsi >= 30 && rsi < 70) fibBuyPrice = fib50;
            // If RSI is overbought (>70), use 61.8% retracement (more discount)
            else fibBuyPrice = fib61_8;

            // Adjust based on MACD trend
            if (macd > 0) // Bullish MACD - less aggressive buy price
            {
                fibBuyPrice = Math.Max(fibBuyPrice, price * 0.95);
            }
            else // Bearish MACD - more aggressive buy price
            {
                fibBuyPrice = Math.Min(fibBuyPrice, price * 0.92);
            }

            // Consider support level
            if (support.HasValue && support.Value > 0)
            {
                double supportZoneTop = support.Value * 1.02;
                double supportZoneBottom = support.Value * 0.98;
                if (price <= supportZoneTop) fibBuyPrice = Math.Max(fibBuyPrice, supportZoneBottom);
                else fibBuyPrice = Math.Max(fibBuyPrice, supportZoneBottom);
            }

            // Adjust for volatility
            if (volatility > 3) fibBuyPrice *= 0.95; // High volatility - more conservative
            else if (volatility > 2) fibBuyPrice *= 0.97;
            else if (volatility < 1) fibBuyPrice *= 1.01; // Low volatility - slightly more aggressive

            // Adjust for discount from 52-week high
            if (discountFromHigh > 25) fibBuyPrice = Math.Max(fibBuyPrice, price * 0.90); // Deep discount - more aggressive
            else if (discountFromHigh > 15) fibBuyPrice = Math.Max(fibBuyPrice, price * 0.93);
            else if (discountFromHigh < 5) fibBuyPrice = Math.Min(fibBuyPrice, price * 0.97); // Near high - conservative

            // Ensure buy price is above 52-week low
            if (week52Low > 0 && fibBuyPrice < week52Low) fibBuyPrice = week52Low;

            return Math.Round(fibBuyPrice, 2);
        }

        private double CalculateTargetPrice(StockData stock)
        {
            if (stock == null || stock.Price <= 0) return 0;

            double price = stock.Price;
            double rsi = stock.RSI ?? 50;
            double macd = stock.MACD ?? 0;
            double volatility = stock.Volatility ?? 0;
            double? resistance = stock.ResistanceLevel;
            double discountFromHigh = stock.DiscountFromHigh;
            double week52High = stock.Week52High;
            double week52Low = stock.Week52Low;

            // Calculate Fibonacci extension levels for target price
            double fib127_2 = price + (week52High - week52Low) * 0.272; // 127.2% extension
            double fib161_8 = price + (week52High - week52Low) * 0.618; // 161.8% extension
            double fib200 = price + (week52High - week52Low) * 1.0; // 200% extension

            // Determine best Fibonacci extension level based on RSI and MACD
            double fibTargetPrice = fib161_8; // Default to 161.8% extension
            
            // If RSI is oversold (<30), use 200% extension (more upside potential)
            if (rsi < 30) fibTargetPrice = fib200;
            // If RSI is neutral (30-70), use 161.8% extension
            else if (rsi >= 30 && rsi < 70) fibTargetPrice = fib161_8;
            // If RSI is overbought (>70), use 127.2% extension (conservative)
            else fibTargetPrice = fib127_2;

            // Adjust based on MACD trend
            if (macd > 0) // Bullish MACD - more aggressive target
            {
                fibTargetPrice *= 1.05;
            }
            else // Bearish MACD - more conservative target
            {
                fibTargetPrice *= 0.95;
            }

            // Consider resistance level
            if (resistance.HasValue && resistance.Value > 0)
            {
                double resistanceZone = resistance.Value * 1.02;
                if (resistance.Value < price) fibTargetPrice = resistanceZone;
                else if ((resistance.Value - price) / price <= 0.10) fibTargetPrice = Math.Min(fibTargetPrice, resistanceZone);
                else fibTargetPrice = Math.Max(fibTargetPrice, resistanceZone);
            }

            // Adjust for volatility
            if (volatility > 3) fibTargetPrice *= 1.08; // High volatility - higher target
            else if (volatility > 2) fibTargetPrice *= 1.05;
            else if (volatility < 1) fibTargetPrice *= 1.02; // Low volatility - slightly higher target

            // Adjust for discount from 52-week high
            if (discountFromHigh >= 30) fibTargetPrice = Math.Max(fibTargetPrice, price * 1.35); // Deep discount - higher target
            else if (discountFromHigh >= 20) fibTargetPrice = Math.Max(fibTargetPrice, price * 1.25);
            else if (discountFromHigh >= 10) fibTargetPrice = Math.Max(fibTargetPrice, price * 1.18);
            else if (discountFromHigh <= 5) fibTargetPrice = Math.Min(fibTargetPrice, price * 1.10); // Near high - conservative

            // Adjust based on market cap
            if (stock.MarketCapCategory == "Large Cap") fibTargetPrice = Math.Min(fibTargetPrice, price * 1.20);
            else if (stock.MarketCapCategory == "Mid Cap") fibTargetPrice = Math.Min(fibTargetPrice, price * 1.30);
            else if (stock.MarketCapCategory == "Small Cap") fibTargetPrice = Math.Min(fibTargetPrice, price * 1.40);

            // Ensure target is at least 5% above current price
            fibTargetPrice = Math.Max(fibTargetPrice, price * 1.05);

            // Cap target at reasonable level based on 52-week high
            if (week52High > 0)
            {
                if (discountFromHigh >= 20)
                {
                    if (fibTargetPrice > week52High * 1.15) fibTargetPrice = week52High * 1.15;
                }
                else
                {
                    if (fibTargetPrice > week52High * 1.10) fibTargetPrice = week52High * 1.10;
                }
            }

            return Math.Round(fibTargetPrice, 2);
        }
    }
}
