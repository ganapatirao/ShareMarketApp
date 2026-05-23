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
        private readonly IMemoryCache _cache;

        public OptionsController(
            OptionsDataService optionsDataService,
            IMemoryCache cache)
        {
            _optionsDataService = optionsDataService;
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
            
            _cache.Set(cacheKey, options, TimeSpan.FromMinutes(5));

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
            
            _cache.Set(cacheKey, options, TimeSpan.FromMinutes(5));

            return options;
        }
    }
}
