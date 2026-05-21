using Microsoft.AspNetCore.Mvc;
using stockmarket_agent.Services;

namespace stockmarket_agent.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly MongoStockService _mongoStockService;

        public HealthController(MongoStockService mongoStockService)
        {
            _mongoStockService = mongoStockService;
        }

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
