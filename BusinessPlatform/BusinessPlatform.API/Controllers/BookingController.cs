using BusinessPlatform.API.Models;
using BusinessPlatform.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BusinessPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public BookingController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet("transport")]
        public async Task<IActionResult> GetTransports()
        {
            var transports = await _context.Transports.Find(_ => true).ToListAsync();
            return Ok(transports);
        }

        [HttpGet("transport/{id}")]
        public async Task<IActionResult> GetTransport(string id)
        {
            var transport = await _context.Transports.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (transport == null)
            {
                return NotFound(new { message = "Transport not found" });
            }
            return Ok(transport);
        }

        [HttpGet("transport/type/{type}")]
        public async Task<IActionResult> GetTransportsByType(string type)
        {
            var transports = await _context.Transports.Find(t => t.Type == type).ToListAsync();
            return Ok(transports);
        }

        [HttpPost("transport")]
        [Authorize]
        public async Task<IActionResult> CreateTransport([FromBody] Transport transport)
        {
            transport.Id = null;
            transport.CreatedAt = DateTime.UtcNow;
            await _context.Transports.InsertOneAsync(transport);
            return Ok(new { message = "Transport created successfully", transport });
        }

        [HttpPut("transport/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateTransport(string id, [FromBody] Transport transport)
        {
            transport.Id = id;
            var result = await _context.Transports.ReplaceOneAsync(t => t.Id == id, transport);
            if (result.MatchedCount == 0)
            {
                return NotFound(new { message = "Transport not found" });
            }
            return Ok(new { message = "Transport updated successfully" });
        }

        [HttpPut("transport/{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateTransportStatus(string id, [FromBody] StatusUpdate update)
        {
            var updateDef = Builders<Transport>.Update.Set(t => t.Status, update.Status);
            var result = await _context.Transports.UpdateOneAsync(t => t.Id == id, updateDef);
            if (result.MatchedCount == 0)
            {
                return NotFound(new { message = "Transport not found" });
            }
            return Ok(new { message = "Transport status updated successfully" });
        }

        [HttpGet("packages")]
        public async Task<IActionResult> GetPackages()
        {
            var packages = await _context.TravelPackages.Find(_ => true).ToListAsync();
            return Ok(packages);
        }

        [HttpGet("packages/{id}")]
        public async Task<IActionResult> GetPackage(string id)
        {
            var package = await _context.TravelPackages.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (package == null)
            {
                return NotFound(new { message = "Package not found" });
            }
            return Ok(package);
        }

        [HttpPost("packages")]
        [Authorize]
        public async Task<IActionResult> CreatePackage([FromBody] TravelPackage package)
        {
            package.Id = null;
            package.CreatedAt = DateTime.UtcNow;
            await _context.TravelPackages.InsertOneAsync(package);
            return Ok(new { message = "Package created successfully", package });
        }

        [HttpPut("packages/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdatePackage(string id, [FromBody] TravelPackage package)
        {
            package.Id = id;
            var result = await _context.TravelPackages.ReplaceOneAsync(p => p.Id == id, package);
            if (result.MatchedCount == 0)
            {
                return NotFound(new { message = "Package not found" });
            }
            return Ok(new { message = "Package updated successfully" });
        }

        [HttpPut("packages/{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdatePackageStatus(string id, [FromBody] StatusUpdate update)
        {
            var updateDef = Builders<TravelPackage>.Update.Set(p => p.Status, update.Status);
            var result = await _context.TravelPackages.UpdateOneAsync(p => p.Id == id, updateDef);
            if (result.MatchedCount == 0)
            {
                return NotFound(new { message = "Package not found" });
            }
            return Ok(new { message = "Package status updated successfully" });
        }

        [HttpGet("movies")]
        public async Task<IActionResult> GetMovies()
        {
            var movies = await _context.Movies.Find(_ => true).ToListAsync();
            return Ok(movies);
        }

        [HttpGet("movies/{id}")]
        public async Task<IActionResult> GetMovie(string id)
        {
            var movie = await _context.Movies.Find(m => m.Id == id).FirstOrDefaultAsync();
            if (movie == null)
            {
                return NotFound(new { message = "Movie not found" });
            }
            return Ok(movie);
        }

        [HttpPost("movies")]
        [Authorize]
        public async Task<IActionResult> CreateMovie([FromBody] Movie movie)
        {
            movie.Id = null;
            movie.CreatedAt = DateTime.UtcNow;
            await _context.Movies.InsertOneAsync(movie);
            return Ok(new { message = "Movie created successfully", movie });
        }

        [HttpPut("movies/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateMovie(string id, [FromBody] Movie movie)
        {
            movie.Id = id;
            var result = await _context.Movies.ReplaceOneAsync(m => m.Id == id, movie);
            if (result.MatchedCount == 0)
            {
                return NotFound(new { message = "Movie not found" });
            }
            return Ok(new { message = "Movie updated successfully" });
        }

        [HttpPut("movies/{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateMovieStatus(string id, [FromBody] StatusUpdate update)
        {
            var updateDef = Builders<Movie>.Update.Set(m => m.Status, update.Status);
            var result = await _context.Movies.UpdateOneAsync(m => m.Id == id, updateDef);
            if (result.MatchedCount == 0)
            {
                return NotFound(new { message = "Movie not found" });
            }
            return Ok(new { message = "Movie status updated successfully" });
        }

        [HttpGet("showtimes")]
        public async Task<IActionResult> GetShowtimes()
        {
            var showtimes = await _context.MovieShowtimes.Find(_ => true).ToListAsync();
            return Ok(showtimes);
        }

        [HttpGet("showtimes/movie/{movieId}")]
        public async Task<IActionResult> GetMovieShowtimes(string movieId)
        {
            var showtimes = await _context.MovieShowtimes.Find(s => s.MovieId == movieId).ToListAsync();
            return Ok(showtimes);
        }

        [HttpPost("showtimes")]
        [Authorize]
        public async Task<IActionResult> CreateShowtime([FromBody] MovieShowtime showtime)
        {
            showtime.Id = null;
            showtime.CreatedAt = DateTime.UtcNow;
            await _context.MovieShowtimes.InsertOneAsync(showtime);
            return Ok(new { message = "Showtime created successfully", showtime });
        }

        [HttpGet("bookings")]
        [Authorize]
        public async Task<IActionResult> GetBookings()
        {
            var bookings = await _context.Bookings.Find(_ => true).ToListAsync();
            return Ok(bookings);
        }

        [HttpGet("bookings/user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserBookings(string userId)
        {
            var bookings = await _context.Bookings.Find(b => b.UserId == userId).ToListAsync();
            return Ok(bookings);
        }

        [HttpPost("bookings")]
        public async Task<IActionResult> CreateBooking([FromBody] Booking booking)
        {
            booking.Id = null;
            booking.CreatedAt = DateTime.UtcNow;
            await _context.Bookings.InsertOneAsync(booking);
            return Ok(new { message = "Booking created successfully", booking });
        }

        [HttpPut("bookings/{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateBookingStatus(string id, [FromBody] StatusUpdate update)
        {
            var updateDef = Builders<Booking>.Update.Set(b => b.Status, update.Status);
            var result = await _context.Bookings.UpdateOneAsync(b => b.Id == id, updateDef);
            if (result.MatchedCount == 0)
            {
                return NotFound(new { message = "Booking not found" });
            }
            return Ok(new { message = "Booking status updated successfully" });
        }

        [HttpDelete("bookings/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteBooking(string id)
        {
            var result = await _context.Bookings.DeleteOneAsync(b => b.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { message = "Booking not found" });
            }
            return Ok(new { message = "Booking deleted successfully" });
        }
    }
}
