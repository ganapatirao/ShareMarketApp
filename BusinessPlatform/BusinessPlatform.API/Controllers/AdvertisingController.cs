using BusinessPlatform.API.Models;
using BusinessPlatform.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BusinessPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdvertisingController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public AdvertisingController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet("ads")]
        public async Task<IActionResult> GetAds()
        {
            var ads = await _context.Advertisements.Find(_ => true).ToListAsync();
            return Ok(ads);
        }

        [HttpGet("ads/{id}")]
        public async Task<IActionResult> GetAd(string id)
        {
            var ad = await _context.Advertisements.Find(a => a.Id == id).FirstOrDefaultAsync();
            if (ad == null)
            {
                return NotFound(new { message = "Advertisement not found" });
            }
            return Ok(ad);
        }

        [HttpGet("ads/user/{userId}")]
        public async Task<IActionResult> GetUserAds(string userId)
        {
            var ads = await _context.Advertisements.Find(a => a.SellerId == userId).ToListAsync();
            return Ok(ads);
        }

        [HttpPost("ads")]
        [Authorize]
        public async Task<IActionResult> CreateAd([FromBody] Advertisement ad)
        {
            ad.Id = null;
            ad.CreatedAt = DateTime.UtcNow;
            await _context.Advertisements.InsertOneAsync(ad);
            return Ok(new { message = "Advertisement created successfully", ad });
        }

        [HttpPut("ads/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateAd(string id, [FromBody] Advertisement ad)
        {
            ad.Id = id;
            var result = await _context.Advertisements.ReplaceOneAsync(a => a.Id == id, ad);
            if (result.MatchedCount == 0)
            {
                return NotFound(new { message = "Advertisement not found" });
            }
            return Ok(new { message = "Advertisement updated successfully" });
        }

        [HttpPut("ads/{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateAdStatus(string id, [FromBody] StatusUpdate update)
        {
            var updateDef = Builders<Advertisement>.Update.Set(a => a.Status, update.Status);
            var result = await _context.Advertisements.UpdateOneAsync(a => a.Id == id, updateDef);
            if (result.MatchedCount == 0)
            {
                return NotFound(new { message = "Advertisement not found" });
            }
            return Ok(new { message = "Advertisement status updated successfully" });
        }

        [HttpDelete("ads/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAd(string id)
        {
            var result = await _context.Advertisements.DeleteOneAsync(a => a.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { message = "Advertisement not found" });
            }
            return Ok(new { message = "Advertisement deleted successfully" });
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.AdCategories.Find(_ => true).ToListAsync();
            return Ok(categories);
        }

        [HttpPost("categories")]
        [Authorize]
        public async Task<IActionResult> CreateCategory([FromBody] AdCategory category)
        {
            category.Id = null;
            category.CreatedAt = DateTime.UtcNow;
            await _context.AdCategories.InsertOneAsync(category);
            return Ok(new { message = "Category created successfully", category });
        }

        [HttpGet("responses/{adId}")]
        public async Task<IActionResult> GetAdResponses(string adId)
        {
            var responses = await _context.AdResponses.Find(r => r.AdId == adId).ToListAsync();
            return Ok(responses);
        }

        [HttpPost("responses")]
        public async Task<IActionResult> CreateResponse([FromBody] AdResponse response)
        {
            response.Id = null;
            response.CreatedAt = DateTime.UtcNow;
            await _context.AdResponses.InsertOneAsync(response);
            return Ok(new { message = "Response created successfully", response });
        }
    }
}
