using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using stockmarket_agent.Models;
using stockmarket_agent.Services;

namespace stockmarket_agent.Controllers
{
    /// <summary>
    /// API endpoints for options data management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OptionsController : ControllerBase
    {
        private readonly OptionsDataService _optionsDataService;
        private readonly NSEOptionsService _nseOptionsService;
        private readonly KiteService _kiteService;
        private readonly YahooFinanceOptionsService _yahooFinanceOptionsService;
        private readonly IMemoryCache _cache;

        public OptionsController(
            OptionsDataService optionsDataService,
            NSEOptionsService nseOptionsService,
            KiteService kiteService,
            YahooFinanceOptionsService yahooFinanceOptionsService,
            IMemoryCache cache)
        {
            _optionsDataService = optionsDataService;
            _nseOptionsService = nseOptionsService;
            _kiteService = kiteService;
            _yahooFinanceOptionsService = yahooFinanceOptionsService;
            _cache = cache;
        }

        /// <summary>
        /// Get all options data
        /// </summary>
        /// <returns>List of options data</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OptionsData>>> GetOptions()
        {
            string cacheKey = "options_data";
            
            if (_cache.TryGetValue(cacheKey, out List<OptionsData>? cachedOptions))
            {
                return cachedOptions ?? new List<OptionsData>();
            }

            var options = await _optionsDataService.GetOptionsDataAsync();
            
            _cache.Set(cacheKey, options, TimeSpan.FromMinutes(1));

            return options;
        }

        /// <summary>
        /// Get options data for a specific symbol
        /// </summary>
        /// <param name="symbol">Stock symbol (e.g., RELIANCE.NS)</param>
        /// <returns>List of options data for the specified symbol</returns>
        [HttpGet("{symbol}")]
        public async Task<ActionResult<IEnumerable<OptionsData>>> GetOptionsBySymbol(string symbol)
        {
            string cacheKey = $"options_{symbol}";
            
            if (_cache.TryGetValue(cacheKey, out List<OptionsData>? cachedOptions))
            {
                return cachedOptions ?? new List<OptionsData>();
            }

            var options = await _optionsDataService.GetOptionsDataBySymbolAsync(symbol);
            
            _cache.Set(cacheKey, options, TimeSpan.FromMinutes(1));

            return options;
        }

        /// <summary>
        /// Get full options chain data for a specific symbol
        /// </summary>
        /// <param name="symbol">Stock symbol (e.g., NIFTY, BANKNIFTY)</param>
        /// <returns>Full options chain data with strike prices</returns>
        [HttpGet("{symbol}/chain")]
        public async Task<ActionResult> GetOptionsChain(string symbol)
        {
            string cacheKey = $"options_chain_{symbol}";
            
            if (_cache.TryGetValue(cacheKey, out object? cachedChain))
            {
                return Ok(cachedChain);
            }

            // Try Kite API first
            var kiteOptionsChain = await _kiteService.GetOptionsChain(symbol);
            if (kiteOptionsChain != null && kiteOptionsChain.Data != null)
            {
                _cache.Set(cacheKey, kiteOptionsChain.Data, TimeSpan.FromMinutes(1));
                return Ok(kiteOptionsChain.Data);
            }

            // Fallback to NSE API
            var nseOptionsChain = await _nseOptionsService.GetOptionsChainAsync(symbol);
            if (nseOptionsChain != null)
            {
                _cache.Set(cacheKey, nseOptionsChain, TimeSpan.FromMinutes(1));
                return Ok(nseOptionsChain);
            }

            // Fallback to Yahoo Finance API
            var yahooOptionsChain = await _yahooFinanceOptionsService.GetOptionsChainAsync(symbol);
            if (yahooOptionsChain != null)
            {
                _cache.Set(cacheKey, yahooOptionsChain, TimeSpan.FromMinutes(1));
                return Ok(yahooOptionsChain);
            }

            return Ok(new object[0]);
        }
    }
}
