using Microsoft.AspNetCore.Mvc;
using stockmarket_agent.Services;

namespace stockmarket_agent.Controllers
{
    /// <summary>
    /// API health check endpoint
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly MongoStockService _mongoStockService;

        public HealthController(MongoStockService mongoStockService)
        {
            _mongoStockService = mongoStockService;
        }

        /// <summary>
        /// Health check endpoint to verify API and database connectivity
        /// </summary>
        /// <returns>Health status with service connectivity information</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var mongoConnection = await _mongoStockService.TestConnectionAsync();
            
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Services = new
                {
                    MongoDB = mongoConnection ? "Connected" : "Disconnected"
                }
            });
        }
    }
}
