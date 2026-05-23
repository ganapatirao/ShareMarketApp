using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.OpenApi;
using stockmarket_agent.Models;
using stockmarket_agent.Services;

namespace stockmarket_agent.Controllers
{
    /// <summary>
    /// API endpoints for stock data management and analysis
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StocksController : ControllerBase
    {
        private readonly MongoStockService _mongoStockService;
        private readonly StockAnalysisService _stockAnalysisService;
        private readonly IMemoryCache _cache;

        public StocksController(
            MongoStockService mongoStockService,
            StockAnalysisService stockAnalysisService,
            IMemoryCache cache)
        {
            _mongoStockService = mongoStockService;
            _stockAnalysisService = stockAnalysisService;
            _cache = cache;
        }

        /// <summary>
        /// Get all stocks with optional filtering and pagination
        /// </summary>
        /// <param name="trend">Filter by trend (Bullish, Bearish, Sideways)</param>
        /// <param name="sector">Filter by sector</param>
        /// <param name="marketCapCategory">Filter by market cap category</param>
        /// <param name="companyName">Filter by company name</param>
        /// <param name="condition">Filter by condition</param>
        /// <param name="sortBy">Sort by field (symbol, price, change, trend, etc.)</param>
        /// <param name="sortOrder">Sort order (asc or desc)</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paginated list of stocks with metadata</returns>
        [HttpGet]
        public async Task<ActionResult<object>> GetStocks(
            [FromQuery] string? trend = null,
            [FromQuery] string? sector = null,
            [FromQuery] string? marketCapCategory = null,
            [FromQuery] string? companyName = null,
            [FromQuery] string? condition = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100)
        {
            string cacheKey = $"stocks_{trend}_{sector}_{marketCapCategory}_{companyName}_{condition}_{sortBy}_{sortOrder}_{page}_{pageSize}";
            
            if (_cache.TryGetValue(cacheKey, out object? cachedResult))
            {
                return cachedResult;
            }

            var stocks = await _mongoStockService.GetStocksByFilterAsync(
                trend, sector, marketCapCategory, companyName, condition);

            // Apply sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                stocks = ApplySorting(stocks, sortBy, sortOrder);
            }

            int totalCount = stocks.Count;
            var pagedStocks = stocks
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new
            {
                data = pagedStocks,
                totalCount = totalCount,
                page = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }

        private List<StockData> ApplySorting(List<StockData> stocks, string sortBy, string sortOrder)
        {
            bool ascending = sortOrder?.ToLower() == "asc";

            return sortBy?.ToLower() switch
            {
                "symbol" => ascending ? stocks.OrderBy(s => s.Symbol).ToList() : stocks.OrderByDescending(s => s.Symbol).ToList(),
                "companyname" => ascending ? stocks.OrderBy(s => s.CompanyName).ToList() : stocks.OrderByDescending(s => s.CompanyName).ToList(),
                "price" => ascending ? stocks.OrderBy(s => s.Price).ToList() : stocks.OrderByDescending(s => s.Price).ToList(),
                "change" => ascending ? stocks.OrderBy(s => s.PriceChangePercentage).ToList() : stocks.OrderByDescending(s => s.PriceChangePercentage).ToList(),
                "trend" => ascending ? stocks.OrderBy(s => GetTrendOrder(s.Trend)).ToList() : stocks.OrderByDescending(s => GetTrendOrder(s.Trend)).ToList(),
                "week52high" => ascending ? stocks.OrderBy(s => s.Week52High).ToList() : stocks.OrderByDescending(s => s.Week52High).ToList(),
                "week52low" => ascending ? stocks.OrderBy(s => s.Week52Low).ToList() : stocks.OrderByDescending(s => s.Week52Low).ToList(),
                "discountfromhigh" => ascending ? stocks.OrderBy(s => s.DiscountFromHigh).ToList() : stocks.OrderByDescending(s => s.DiscountFromHigh).ToList(),
                "volume" => ascending ? stocks.OrderBy(s => s.Volume).ToList() : stocks.OrderByDescending(s => s.Volume).ToList(),
                "buyprice" => ascending ? stocks.OrderBy(s => s.BuyPrice).ToList() : stocks.OrderByDescending(s => s.BuyPrice).ToList(),
                "targetprice" => ascending ? stocks.OrderBy(s => s.TargetPrice).ToList() : stocks.OrderByDescending(s => s.TargetPrice).ToList(),
                "rsi" => ascending ? stocks.OrderBy(s => s.RSI).ToList() : stocks.OrderByDescending(s => s.RSI).ToList(),
                "sector" => ascending ? stocks.OrderBy(s => s.Sector).ToList() : stocks.OrderByDescending(s => s.Sector).ToList(),
                "marketcap" => ascending ? stocks.OrderBy(s => s.MarketCapCategory).ToList() : stocks.OrderByDescending(s => s.MarketCapCategory).ToList(),
                _ => stocks
            };
        }

        private int GetTrendOrder(string? trend)
        {
            return trend?.ToLower() switch
            {
                "bullish" => 1,
                "sideways" => 2,
                "bearish" => 3,
                _ => 4
            };
        }

        /// <summary>
        /// Get a specific stock by symbol
        /// </summary>
        /// <param name="symbol">Stock symbol (e.g., RELIANCE.NS)</param>
        /// <returns>Stock data for the specified symbol</returns>
        [HttpGet("{symbol}")]
        public async Task<ActionResult<StockData>> GetStock(string symbol)
        {
            string cacheKey = $"stock_{symbol}";
            
            if (_cache.TryGetValue(cacheKey, out StockData? cachedStock))
            {
                if (cachedStock != null)
                    return cachedStock;
                return NotFound();
            }

            var stock = await _mongoStockService.GetStockBySymbolAsync(symbol);
            
            if (stock == null)
            {
                return NotFound();
            }

            _cache.Set(cacheKey, stock, TimeSpan.FromMinutes(5));

            return stock;
        }

        /// <summary>
        /// Get all unique company names
        /// </summary>
        /// <returns>List of unique company names</returns>
        [HttpGet("company-names")]
        public async Task<ActionResult<IEnumerable<string>>> GetCompanyNames()
        {
            string cacheKey = "company_names";
            
            if (_cache.TryGetValue(cacheKey, out List<string>? cachedNames))
            {
                return cachedNames ?? new List<string>();
            }

            var stocks = await _mongoStockService.GetAllStocksAsync();
            var names = stocks.Select(s => s.CompanyName).Distinct().ToList();

            _cache.Set(cacheKey, names, TimeSpan.FromMinutes(5));

            return names;
        }

        /// <summary>
        /// Get all unique sectors
        /// </summary>
        /// <returns>List of unique sectors</returns>
        [HttpGet("sectors")]
        public async Task<ActionResult<IEnumerable<string>>> GetSectors()
        {
            string cacheKey = "sectors";
            
            if (_cache.TryGetValue(cacheKey, out List<string>? cachedSectors))
            {
                return cachedSectors ?? new List<string>();
            }

            var stocks = await _mongoStockService.GetAllStocksAsync();
            var sectors = stocks.Select(s => s.Sector).Where(s => !string.IsNullOrEmpty(s)).Distinct().OrderBy(s => s).ToList();

            _cache.Set(cacheKey, sectors, TimeSpan.FromMinutes(5));

            return sectors;
        }

        /// <summary>
        /// Get all unique market cap categories
        /// </summary>
        /// <returns>List of unique market cap categories</returns>
        [HttpGet("market-caps")]
        public async Task<ActionResult<IEnumerable<string>>> GetMarketCaps()
        {
            string cacheKey = "market_caps";
            
            if (_cache.TryGetValue(cacheKey, out List<string>? cachedMarketCaps))
            {
                return cachedMarketCaps ?? new List<string>();
            }

            var stocks = await _mongoStockService.GetAllStocksAsync();
            var marketCaps = stocks.Select(s => s.MarketCapCategory).Where(s => !string.IsNullOrEmpty(s)).Distinct().OrderBy(s => s).ToList();

            _cache.Set(cacheKey, marketCaps, TimeSpan.FromMinutes(5));

            return marketCaps;
        }

        /// <summary>
        /// Get technical analysis for a specific stock
        /// </summary>
        /// <param name="symbol">Stock symbol (e.g., RELIANCE.NS)</param>
        /// <returns>Stock data with technical analysis indicators</returns>
        [HttpGet("{symbol}/technical-analysis")]
        public async Task<ActionResult<StockData>> GetTechnicalAnalysis(string symbol)
        {
            var stock = await _mongoStockService.GetStockBySymbolAsync(symbol);
            
            if (stock == null)
            {
                return NotFound();
            }

            var analyzedStock = _stockAnalysisService.AnalyzeStock(stock);
            
            return analyzedStock;
        }

        /// <summary>
        /// Compare multiple stocks side by side
        /// </summary>
        /// <param name="symbols">Comma-separated list of stock symbols (e.g., RELIANCE.NS,TCS.NS)</param>
        /// <returns>List of stocks with technical analysis for comparison</returns>
        [HttpGet("compare")]
        public async Task<ActionResult<IEnumerable<StockData>>> CompareStocks([FromQuery] string symbols)
        {
            if (string.IsNullOrEmpty(symbols))
            {
                return BadRequest("Symbols parameter is required");
            }

            var symbolList = symbols.Split(',').Select(s => s.Trim()).ToList();
            var stocks = new List<StockData>();

            foreach (var symbol in symbolList)
            {
                var stock = await _mongoStockService.GetStockBySymbolAsync(symbol);
                if (stock != null)
                {
                    var analyzedStock = _stockAnalysisService.AnalyzeStock(stock);
                    stocks.Add(analyzedStock);
                }
            }

            return stocks;
        }

        /// <summary>
        /// Get sector-wise analysis of stocks
        /// </summary>
        /// <returns>Sector analysis with count, average price, average change, and trend distribution</returns>
        [HttpGet("sector-analysis")]
        public async Task<ActionResult<IEnumerable<object>>> GetSectorAnalysis()
        {
            var stocks = await _mongoStockService.GetAllStocksAsync();
            
            var sectorAnalysis = stocks
                .GroupBy(s => s.Sector)
                .Select(g => new
                {
                    Sector = g.Key,
                    Count = g.Count(),
                    AveragePrice = g.Average(s => s.Price),
                    AverageChange = g.Average(s => s.PriceChangePercentage),
                    BullishCount = g.Count(s => s.Trend == "Bullish"),
                    BearishCount = g.Count(s => s.Trend == "Bearish"),
                    SidewaysCount = g.Count(s => s.Trend == "Sideways")
                })
                .ToList();

            return sectorAnalysis;
        }

        /// <summary>
        /// Get multibagger stocks (stocks with >20% price change)
        /// </summary>
        /// <returns>Top 20 stocks with highest price change percentage</returns>
        [HttpGet("multibagger")]
        public async Task<ActionResult<IEnumerable<StockData>>> GetMultibaggerStocks()
        {
            var stocks = await _mongoStockService.GetAllStocksAsync();
            
            var multibaggers = stocks
                .Where(s => s.PriceChangePercentage > 20)
                .OrderByDescending(s => s.PriceChangePercentage)
                .Take(20)
                .ToList();

            return multibaggers;
        }

        /// <summary>
        /// Get valuable stocks (stocks with more than 15 percent discount from 52-week high and RSI less than 40)
        /// </summary>
        /// <returns>Top 20 stocks with highest discount from 52-week high and oversold RSI</returns>
        [HttpGet("valuable")]
        public async Task<ActionResult<IEnumerable<StockData>>> GetValuableStocks()
        {
            var stocks = await _mongoStockService.GetAllStocksAsync();
            
            var valuableStocks = stocks
                .Where(s => s.DiscountFromHigh > 15 && s.RSI.HasValue && s.RSI < 40)
                .OrderBy(s => s.DiscountFromHigh)
                .Take(20)
                .ToList();

            return valuableStocks;
        }

        /// <summary>
        /// Reseed the stock collection with initial data
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("reseed")]
        public async Task<ActionResult> ReseedStocks()
        {
            await _mongoStockService.ClearStockCollectionAsync();
            await _mongoStockService.SeedInitialDataAsync();
            return Ok(new { message = "Stock collection reseeded successfully" });
        }
    }
}
