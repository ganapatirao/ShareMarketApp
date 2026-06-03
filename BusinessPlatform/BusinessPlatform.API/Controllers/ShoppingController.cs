using BusinessPlatform.API.Models;
using BusinessPlatform.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BusinessPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShoppingController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public ShoppingController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.Find(_ => true).ToListAsync();
            return Ok(products);
        }

        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetProduct(string id)
        {
            var product = await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }
            return Ok(product);
        }

        [HttpPost("products")]
        [Authorize]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            product.Id = null;
            product.CreatedAt = DateTime.UtcNow;
            await _context.Products.InsertOneAsync(product);
            return Ok(new { message = "Product created successfully", product });
        }

        [HttpPut("products/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] Product product)
        {
            product.Id = id;
            var result = await _context.Products.ReplaceOneAsync(p => p.Id == id, product);
            if (result.MatchedCount == 0)
            {
                return NotFound(new { message = "Product not found" });
            }
            return Ok(new { message = "Product updated successfully" });
        }

        [HttpPut("products/{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateProductStatus(string id, [FromBody] StatusUpdate update)
        {
            var updateDef = Builders<Product>.Update.Set(p => p.Status, update.Status);
            var result = await _context.Products.UpdateOneAsync(p => p.Id == id, updateDef);
            if (result.MatchedCount == 0)
            {
                return NotFound(new { message = "Product not found" });
            }
            return Ok(new { message = "Product status updated successfully" });
        }

        [HttpDelete("products/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var result = await _context.Products.DeleteOneAsync(p => p.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { message = "Product not found" });
            }
            return Ok(new { message = "Product deleted successfully" });
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.Find(_ => true).ToListAsync();
            return Ok(categories);
        }

        [HttpPost("categories")]
        [Authorize]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            category.Id = null;
            category.CreatedAt = DateTime.UtcNow;
            await _context.Categories.InsertOneAsync(category);
            return Ok(new { message = "Category created successfully", category });
        }

        [HttpPost("orders")]
        public async Task<IActionResult> CreateOrder([FromBody] ShoppingOrder order)
        {
            order.Id = null;
            order.CreatedAt = DateTime.UtcNow;
            await _context.ShoppingOrders.InsertOneAsync(order);
            
            // Clear cart
            await _context.ShoppingCartItems.DeleteManyAsync(c => c.UserId == order.UserId);
            
            return Ok(new { message = "Order created successfully", order });
        }

        [HttpGet("orders")]
        [Authorize]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.ShoppingOrders.Find(_ => true).ToListAsync();
            return Ok(orders);
        }

        [HttpGet("orders/user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserOrders(string userId)
        {
            var orders = await _context.ShoppingOrders.Find(o => o.UserId == userId).ToListAsync();
            return Ok(orders);
        }

        [HttpPut("orders/{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] StatusUpdate update)
        {
            var updateDef = Builders<ShoppingOrder>.Update.Set(o => o.Status, update.Status);
            var result = await _context.ShoppingOrders.UpdateOneAsync(o => o.Id == id, updateDef);
            if (result.MatchedCount == 0)
            {
                return NotFound(new { message = "Order not found" });
            }
            return Ok(new { message = "Order status updated successfully" });
        }

        [HttpDelete("orders/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            var result = await _context.ShoppingOrders.DeleteOneAsync(o => o.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { message = "Order not found" });
            }
            return Ok(new { message = "Order deleted successfully" });
        }

        [HttpGet("cart/{userId}")]
        public async Task<IActionResult> GetCart(string userId)
        {
            var cartItems = await _context.ShoppingCartItems.Find(c => c.UserId == userId).ToListAsync();
            return Ok(cartItems);
        }

        [HttpPost("cart")]
        public async Task<IActionResult> AddToCart([FromBody] ShoppingCartItem item)
        {
            item.Id = null;
            item.CreatedAt = DateTime.UtcNow;
            
            var existing = await _context.ShoppingCartItems
                .Find(c => c.UserId == item.UserId && c.ProductId == item.ProductId)
                .FirstOrDefaultAsync();
            
            if (existing != null)
            {
                var updateDef = Builders<ShoppingCartItem>.Update.Inc(c => c.Quantity, item.Quantity);
                await _context.ShoppingCartItems.UpdateOneAsync(c => c.Id == existing.Id, updateDef);
            }
            else
            {
                await _context.ShoppingCartItems.InsertOneAsync(item);
            }
            
            return Ok(new { message = "Item added to cart successfully" });
        }

        [HttpDelete("cart/{id}")]
        public async Task<IActionResult> RemoveFromCart(string id)
        {
            var result = await _context.ShoppingCartItems.DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { message = "Cart item not found" });
            }
            return Ok(new { message = "Item removed from cart successfully" });
        }

        [HttpDelete("cart/user/{userId}")]
        public async Task<IActionResult> ClearCart(string userId)
        {
            await _context.ShoppingCartItems.DeleteManyAsync(c => c.UserId == userId);
            return Ok(new { message = "Cart cleared successfully" });
        }
    }

    public class StatusUpdate
    {
        public string Status { get; set; } = string.Empty;
    }
}
