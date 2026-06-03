using System.Security.Claims;
using System.Text;
using BusinessPlatform.API.Models;
using BusinessPlatform.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using MongoDB.Driver;

namespace BusinessPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly IConfiguration _configuration;

        public AdminController(MongoDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private string HashPassword(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id!),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
            if (user == null || user.Password != HashPassword(request.Password))
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Account is inactive" });
            }

            var token = GenerateJwtToken(user);
            return Ok(new { token, user = new { user.Id, user.Email, user.FullName, user.Role } });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var existingUser = await _context.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email already registered" });
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Password = HashPassword(request.Password),
                FullName = request.FullName,
                Phone = request.Phone,
                Role = "User",
                IsActive = true
            };

            await _context.Users.InsertOneAsync(user);
            return Ok(new { message = "User registered successfully", user });
        }

        [HttpGet("users")]
        [Authorize]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.Find(_ => true).ToListAsync();
            return Ok(users);
        }

        [HttpGet("users/{id}")]
        [Authorize]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }
            return Ok(user);
        }

        [HttpPut("users/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User user)
        {
            user.Id = id;
            var result = await _context.Users.ReplaceOneAsync(u => u.Id == id, user);
            if (result.MatchedCount == 0)
            {
                return NotFound(new { message = "User not found" });
            }
            return Ok(new { message = "User updated successfully" });
        }

        [HttpDelete("users/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _context.Users.DeleteOneAsync(u => u.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { message = "User not found" });
            }
            return Ok(new { message = "User deleted successfully" });
        }

        [HttpGet("dashboard")]
        [Authorize]
        public async Task<IActionResult> GetDashboard()
        {
            var totalUsers = (int)await _context.Users.CountDocumentsAsync(FilterDefinition<User>.Empty);
            var totalProducts = (int)await _context.Products.CountDocumentsAsync(FilterDefinition<Product>.Empty);
            var totalOrders = (int)await _context.ShoppingOrders.CountDocumentsAsync(FilterDefinition<ShoppingOrder>.Empty);
            var totalAds = (int)await _context.Advertisements.CountDocumentsAsync(FilterDefinition<Advertisement>.Empty);
            var totalJobs = (int)await _context.Jobs.CountDocumentsAsync(FilterDefinition<Job>.Empty);
            var totalBookings = (int)await _context.Bookings.CountDocumentsAsync(FilterDefinition<Booking>.Empty);

            var orders = await _context.ShoppingOrders.Find(FilterDefinition<ShoppingOrder>.Empty).ToListAsync();
            var totalRevenue = orders.Where(o => o.Status == "Delivered").Sum(o => o.Total);

            var dashboard = new AdminDashboard
            {
                TotalUsers = (int)totalUsers,
                TotalProducts = (int)totalProducts,
                TotalOrders = (int)totalOrders,
                TotalAds = (int)totalAds,
                TotalJobs = (int)totalJobs,
                TotalBookings = (int)totalBookings,
                TotalRevenue = (double)totalRevenue
            };

            return Ok(dashboard);
        }

        [HttpGet("orders/all")]
        [Authorize]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.ShoppingOrders.Find(FilterDefinition<ShoppingOrder>.Empty).ToListAsync();
            return Ok(orders);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
