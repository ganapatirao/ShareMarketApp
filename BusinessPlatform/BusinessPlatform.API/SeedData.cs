using BusinessPlatform.API.Models;
using BusinessPlatform.API.Services;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

namespace BusinessPlatform.API
{
    public class SeedData
    {
        private readonly MongoDbContext _context;

        public SeedData(MongoDbContext context)
        {
            _context = context;
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public async Task SeedAsync()
        {
            // Clear all collections
            await _context.Users.DeleteManyAsync(_ => true);
            await _context.Categories.DeleteManyAsync(_ => true);
            await _context.Products.DeleteManyAsync(_ => true);
            await _context.ShoppingCartItems.DeleteManyAsync(_ => true);
            await _context.ShoppingOrders.DeleteManyAsync(_ => true);
            await _context.AdCategories.DeleteManyAsync(_ => true);
            await _context.Advertisements.DeleteManyAsync(_ => true);
            await _context.AdResponses.DeleteManyAsync(_ => true);
            await _context.Jobs.DeleteManyAsync(_ => true);
            await _context.JobApplications.DeleteManyAsync(_ => true);
            await _context.Candidates.DeleteManyAsync(_ => true);
            await _context.Transports.DeleteManyAsync(_ => true);
            await _context.TravelPackages.DeleteManyAsync(_ => true);
            await _context.Movies.DeleteManyAsync(_ => true);
            await _context.MovieShowtimes.DeleteManyAsync(_ => true);
            await _context.Bookings.DeleteManyAsync(_ => true);

            // Seed Users
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@businessplatform.com",
                Password = HashPassword("admin123"),
                FullName = "System Administrator",
                Phone = "+1-555-0100",
                Role = "Admin",
                IsActive = true
            };
            await _context.Users.InsertOneAsync(adminUser);

            var user1 = new User
            {
                Username = "johnsmith",
                Email = "john@example.com",
                Password = HashPassword("password123"),
                FullName = "John Smith",
                Phone = "+1-555-0101",
                Role = "User",
                IsActive = true
            };
            await _context.Users.InsertOneAsync(user1);

            var user2 = new User
            {
                Username = "janedoe",
                Email = "jane@example.com",
                Password = HashPassword("password123"),
                FullName = "Jane Doe",
                Phone = "+1-555-0102",
                Role = "User",
                IsActive = true
            };
            await _context.Users.InsertOneAsync(user2);

            var user3 = new User
            {
                Username = "bobwilson",
                Email = "bob@example.com",
                Password = HashPassword("password123"),
                FullName = "Bob Wilson",
                Phone = "+1-555-0103",
                Role = "User",
                IsActive = true
            };
            await _context.Users.InsertOneAsync(user3);

            var user4 = new User
            {
                Username = "alicebrown",
                Email = "alice@example.com",
                Password = HashPassword("password123"),
                FullName = "Alice Brown",
                Phone = "+1-555-0104",
                Role = "User",
                IsActive = true
            };
            await _context.Users.InsertOneAsync(user4);

            // Seed Categories
            var categories = new[]
            {
                new Category { Name = "Electronics", Description = "Electronic devices and gadgets", ImageUrl = "https://images.unsplash.com/photo-1498049794561-7780e7231661?w=400&h=300&fit=crop" },
                new Category { Name = "Clothing", Description = "Fashion and apparel", ImageUrl = "https://images.unsplash.com/photo-1445205170230-053b83016050?w=400&h=300&fit=crop" },
                new Category { Name = "Books", Description = "Books and educational materials", ImageUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=400&h=300&fit=crop" },
                new Category { Name = "Home & Kitchen", Description = "Home appliances and kitchen items", ImageUrl = "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400&h=300&fit=crop" },
                new Category { Name = "Sports", Description = "Sports equipment and accessories", ImageUrl = "https://images.unsplash.com/photo-1461896836934-voices-of-sports?w=400&h=300&fit=crop" }
            };
            await _context.Categories.InsertManyAsync(categories);

            // Seed Products
            var products = new[]
            {
                new Product { Name = "Wireless Bluetooth Headphones", Description = "High-quality wireless headphones with noise cancellation and 30-hour battery life.", Price = 149.99m, CategoryName = "Electronics", Stock = 50, Seller = "John Smith", Rating = 4.5, ReviewCount = 128, ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600&h=400&fit=crop" },
                new Product { Name = "Smart Watch Series 5", Description = "Advanced smartwatch with health monitoring, GPS, and water resistance.", Price = 299.99m, CategoryName = "Electronics", Stock = 30, Seller = "John Smith", Rating = 4.8, ReviewCount = 256, ImageUrl = "https://images.unsplash.com/photo-1546868871-7041f2a55e12?w=600&h=400&fit=crop" },
                new Product { Name = "Premium Cotton T-Shirt", Description = "100% organic cotton t-shirt, comfortable and durable.", Price = 29.99m, CategoryName = "Clothing", Stock = 200, Seller = "Jane Doe", Rating = 4.2, ReviewCount = 89, ImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=600&h=400&fit=crop" },
                new Product { Name = "Running Shoes Pro", Description = "Professional running shoes with advanced cushioning technology.", Price = 129.99m, CategoryName = "Sports", Stock = 75, Seller = "Bob Wilson", Rating = 4.6, ReviewCount = 145, ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=600&h=400&fit=crop" },
                new Product { Name = "Programming Guide: C#", Description = "Comprehensive guide to C# programming for beginners and advanced developers.", Price = 49.99m, CategoryName = "Books", Stock = 100, Seller = "Alice Brown", Rating = 4.7, ReviewCount = 67, ImageUrl = "https://images.unsplash.com/photo-1532012197267-da84d127e765?w=600&h=400&fit=crop" },
                new Product { Name = "4K Smart TV 55 inch", Description = "Ultra HD smart TV with streaming apps and voice control.", Price = 599.99m, CategoryName = "Electronics", Stock = 25, Seller = "John Smith", Rating = 4.4, ReviewCount = 198, ImageUrl = "https://images.unsplash.com/photo-1593359677879-a4bb92f829d1?w=600&h=400&fit=crop" },
                new Product { Name = "Coffee Maker Deluxe", Description = "Programmable coffee maker with built-in grinder and thermal carafe.", Price = 89.99m, CategoryName = "Home & Kitchen", Stock = 60, Seller = "Jane Doe", Rating = 4.3, ReviewCount = 112, ImageUrl = "https://images.unsplash.com/photo-1517668808822-9ebb02f2a0e6?w=600&h=400&fit=crop" },
                new Product { Name = "Yoga Mat Premium", Description = "Extra thick yoga mat with non-slip surface and carrying strap.", Price = 39.99m, CategoryName = "Sports", Stock = 150, Seller = "Bob Wilson", Rating = 4.5, ReviewCount = 78, ImageUrl = "https://images.unsplash.com/photo-1601925260368-ae2f83cf8b7f?w=600&h=400&fit=crop" }
            };
            await _context.Products.InsertManyAsync(products);

            // Seed Shopping Orders
            var orders = new[]
            {
                new ShoppingOrder { UserId = user1.Id, UserName = user1.FullName, Items = new List<OrderItem> { new OrderItem { ProductId = products[0].Id, ProductName = products[0].Name, Quantity = 2, Price = products[0].Price }, new OrderItem { ProductId = products[2].Id, ProductName = products[2].Name, Quantity = 3, Price = products[2].Price } }, Total = 389.95m, Status = "Delivered", ShippingAddress = "123 Main St, New York, NY", PaymentMethod = "Credit Card" },
                new ShoppingOrder { UserId = user2.Id, UserName = user2.FullName, Items = new List<OrderItem> { new OrderItem { ProductId = products[1].Id, ProductName = products[1].Name, Quantity = 1, Price = products[1].Price } }, Total = 299.99m, Status = "Shipped", ShippingAddress = "456 Oak Ave, Los Angeles, CA", PaymentMethod = "PayPal" },
                new ShoppingOrder { UserId = user3.Id, UserName = user3.FullName, Items = new List<OrderItem> { new OrderItem { ProductId = products[3].Id, ProductName = products[3].Name, Quantity = 1, Price = products[3].Price }, new OrderItem { ProductId = products[6].Id, ProductName = products[6].Name, Quantity = 1, Price = products[6].Price } }, Total = 219.98m, Status = "Confirmed", ShippingAddress = "789 Pine Rd, Chicago, IL", PaymentMethod = "Credit Card" }
            };
            await _context.ShoppingOrders.InsertManyAsync(orders);

            // Seed Ad Categories
            var adCategories = new[]
            {
                new AdCategory { Name = "Electronics", Emoji = "📱" },
                new AdCategory { Name = "Vehicles", Emoji = "🚗" },
                new AdCategory { Name = "Real Estate", Emoji = "🏠" },
                new AdCategory { Name = "Furniture", Emoji = "🪑" },
                new AdCategory { Name = "Jobs", Emoji = "💼" },
                new AdCategory { Name = "Services", Emoji = "🔧" }
            };
            await _context.AdCategories.InsertManyAsync(adCategories);

            // Seed Advertisements
            var ads = new[]
            {
                new Advertisement { Title = "iPhone 14 Pro - Excellent Condition", Description = "Like new iPhone 14 Pro, 256GB, comes with original box and accessories.", Price = 799.99m, CategoryName = "Electronics", Location = "New York, NY", Condition = "Like New", SellerId = user1.Id, SellerName = user1.FullName, ImageUrl = "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=600&h=400&fit=crop" },
                new Advertisement { Title = "2019 Toyota Camry - Low Mileage", Description = "Well-maintained 2019 Toyota Camry with only 35,000 miles. Clean title.", Price = 18500.00m, CategoryName = "Vehicles", Location = "Los Angeles, CA", Condition = "Good", SellerId = user2.Id, SellerName = user2.FullName, ImageUrl = "https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=600&h=400&fit=crop" },
                new Advertisement { Title = "Modern Sofa Set - Brand New", Description = "Beautiful 3-piece sofa set, gray fabric, never used.", Price = 599.99m, CategoryName = "Furniture", Location = "Chicago, IL", Condition = "New", SellerId = user3.Id, SellerName = user3.FullName, ImageUrl = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=600&h=400&fit=crop" },
                new Advertisement { Title = "2BR Apartment for Rent", Description = "Spacious 2-bedroom apartment in downtown area, close to amenities.", Price = 1500.00m, CategoryName = "Real Estate", Location = "Houston, TX", Condition = "Good", SellerId = user4.Id, SellerName = user4.FullName, ImageUrl = "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=600&h=400&fit=crop" },
                new Advertisement { Title = "MacBook Pro 2021 - Refurbished", Description = "MacBook Pro 14-inch M1 Pro, 16GB RAM, 512GB SSD, excellent condition.", Price = 1499.99m, CategoryName = "Electronics", Location = "Seattle, WA", Condition = "Good", SellerId = user1.Id, SellerName = user1.FullName, ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=600&h=400&fit=crop" }
            };
            await _context.Advertisements.InsertManyAsync(ads);

            // Seed Ad Responses
            var adResponses = new[]
            {
                new AdResponse { AdId = ads[0].Id, ResponderId = user2.Id, ResponderName = user2.FullName, Message = "Is this still available? I'm interested." },
                new AdResponse { AdId = ads[1].Id, ResponderId = user3.Id, ResponderName = user3.FullName, Message = "Can you provide more details about the car's history?" }
            };
            await _context.AdResponses.InsertManyAsync(adResponses);

            // Seed Jobs
            var jobs = new[]
            {
                new Job { Title = "Senior Software Engineer", Company = "TechCorp Inc.", Location = "San Francisco, CA", Salary = "$120K-$160K", Type = "Full-time", Description = "We are looking for an experienced software engineer to join our team.", Skills = new List<string> { "C#", ".NET", "React", "MongoDB" } },
                new Job { Title = "Product Manager", Company = "InnovateTech", Location = "New York, NY", Salary = "$110K-$140K", Type = "Full-time", Description = "Lead product development and strategy for our SaaS platform.", Skills = new List<string> { "Product Management", "Agile", "Strategy" } },
                new Job { Title = "UX Designer", Company = "DesignHub", Location = "Remote", Salary = "$80K-$110K", Type = "Remote", Description = "Create beautiful and intuitive user experiences for our clients.", Skills = new List<string> { "Figma", "UI/UX", "Prototyping" } },
                new Job { Title = "Data Analyst Intern", Company = "DataDriven Co.", Location = "Austin, TX", Salary = "$30K-$40K", Type = "Internship", Description = "Learn data analysis and visualization techniques.", Skills = new List<string> { "Python", "SQL", "Excel" } },
                new Job { Title = "DevOps Engineer", Company = "CloudScale", Location = "Seattle, WA", Salary = "$130K-$170K", Type = "Full-time", Description = "Build and maintain cloud infrastructure and CI/CD pipelines.", Skills = new List<string> { "AWS", "Docker", "Kubernetes", "Jenkins" } }
            };
            await _context.Jobs.InsertManyAsync(jobs);

            // Seed Candidates
            var candidates = new[]
            {
                new Candidate { Name = "John Smith", Email = "john@example.com", Phone = "+1-555-0101", Experience = "6 years", Skills = new List<string> { "C#", ".NET", "React", "MongoDB" }, Resume = "john_smith_resume.pdf" },
                new Candidate { Name = "Jane Doe", Email = "jane@example.com", Phone = "+1-555-0102", Experience = "5 years", Skills = new List<string> { "Product Management", "Agile", "Strategy" }, Resume = "jane_doe_resume.pdf" },
                new Candidate { Name = "Bob Wilson", Email = "bob@example.com", Phone = "+1-555-0103", Experience = "4 years", Skills = new List<string> { "Figma", "UI/UX", "Prototyping" }, Resume = "bob_wilson_resume.pdf" }
            };
            await _context.Candidates.InsertManyAsync(candidates);

            // Seed Job Applications
            var applications = new[]
            {
                new JobApplication { JobId = jobs[0].Id, JobTitle = jobs[0].Title, ApplicantId = candidates[0].Id, ApplicantName = candidates[0].Name, Email = candidates[0].Email, Phone = candidates[0].Phone, Resume = candidates[0].Resume, CoverLetter = "I am excited to apply for this position...", Status = "Shortlisted" },
                new JobApplication { JobId = jobs[1].Id, JobTitle = jobs[1].Title, ApplicantId = candidates[1].Id, ApplicantName = candidates[1].Name, Email = candidates[1].Email, Phone = candidates[1].Phone, Resume = candidates[1].Resume, CoverLetter = "With my experience in product management...", Status = "Reviewed" },
                new JobApplication { JobId = jobs[2].Id, JobTitle = jobs[2].Title, ApplicantId = candidates[2].Id, ApplicantName = candidates[2].Name, Email = candidates[2].Email, Phone = candidates[2].Phone, Resume = candidates[2].Resume, CoverLetter = "I have a passion for creating great user experiences...", Status = "Pending" }
            };
            await _context.JobApplications.InsertManyAsync(applications);

            // Seed Transports
            var transports = new[]
            {
                new Transport { Type = "Train", Name = "Express Rail 202", Source = "New York", Destination = "Washington DC", Price = 89.99m, Duration = "3h 30m", Operator = "Amtrak" },
                new Transport { Type = "Bus", Name = "Luxury Coach Express", Source = "Boston", Destination = "New York", Price = 45.99m, Duration = "4h 15m", Operator = "Greyhound" },
                new Transport { Type = "Car", Name = "Premium Sedan Service", Source = "Los Angeles", Destination = "San Diego", Price = 199.99m, Duration = "2h 30m", Operator = "Uber Black" },
                new Transport { Type = "Bike", Name = "Motorcycle Tour", Source = "San Francisco", Destination = "Monterey", Price = 79.99m, Duration = "2h 45m", Operator = "Harley Tours" }
            };
            await _context.Transports.InsertManyAsync(transports);

            // Seed Travel Packages
            var packages = new[]
            {
                new TravelPackage { Name = "European Adventure", Description = "Experience the best of Europe with visits to Paris, Rome, and Barcelona.", Duration = "10 days", Price = 2999.99m, Destinations = new List<string> { "Paris", "Rome", "Barcelona" }, ImageUrl = "https://images.unsplash.com/photo-1469854523086-cc02fe5d8800?w=600&h=400&fit=crop" },
                new TravelPackage { Name = "Tropical Paradise Getaway", Description = "Relax in the beautiful Maldives with pristine beaches and luxury resorts.", Duration = "7 days", Price = 4499.99m, Destinations = new List<string> { "Maldives" }, ImageUrl = "https://images.unsplash.com/photo-1514282401047-d79a71a590e8?w=600&h=400&fit=crop" },
                new TravelPackage { Name = "Asian Cultural Experience", Description = "Discover the rich culture of Japan with visits to Tokyo, Kyoto, and Osaka.", Duration = "14 days", Price = 3499.99m, Destinations = new List<string> { "Tokyo", "Kyoto", "Osaka" }, ImageUrl = "https://images.unsplash.com/photo-1493976040374-85c8e12f0c0e?w=600&h=400&fit=crop" }
            };
            await _context.TravelPackages.InsertManyAsync(packages);

            // Seed Movies
            var movies = new[]
            {
                new Movie { Title = "The Quantum Paradox", Genre = "Sci-Fi", Language = "English", Duration = 148, Rating = 8.5, ImageUrl = "https://images.unsplash.com/photo-1536440136628-849c177e76a1?w=600&h=400&fit=crop" },
                new Movie { Title = "Midnight in Paris", Genre = "Romance", Language = "French", Duration = 124, Rating = 7.8, ImageUrl = "https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?w=600&h=400&fit=crop" },
                new Movie { Title = "The Last Samurai Returns", Genre = "Action", Language = "Japanese", Duration = 156, Rating = 9.1, ImageUrl = "https://images.unsplash.com/photo-1535016120720-40c646be5580?w=600&h=400&fit=crop" },
                new Movie { Title = "Comedy Night", Genre = "Comedy", Language = "English", Duration = 112, Rating = 7.2, ImageUrl = "https://images.unsplash.com/photo-1485846234645-a62644f84728?w=600&h=400&fit=crop" }
            };
            await _context.Movies.InsertManyAsync(movies);

            // Seed Movie Showtimes
            var showtimes = new[]
            {
                new MovieShowtime { MovieId = movies[0].Id, MovieTitle = movies[0].Title, Theater = "Cineplex Downtown", ShowDate = DateTime.Today.AddDays(1), ShowTime = "14:00", ScreenType = "IMAX", Price = 15.99m },
                new MovieShowtime { MovieId = movies[0].Id, MovieTitle = movies[0].Title, Theater = "Cineplex Downtown", ShowDate = DateTime.Today.AddDays(1), ShowTime = "18:00", ScreenType = "Standard", Price = 12.99m },
                new MovieShowtime { MovieId = movies[1].Id, MovieTitle = movies[1].Title, Theater = "Metro Cinema", ShowDate = DateTime.Today.AddDays(1), ShowTime = "16:00", ScreenType = "Standard", Price = 11.99m },
                new MovieShowtime { MovieId = movies[2].Id, MovieTitle = movies[2].Title, Theater = "Grand Theater", ShowDate = DateTime.Today.AddDays(1), ShowTime = "20:00", ScreenType = "IMAX", Price = 16.99m },
                new MovieShowtime { MovieId = movies[3].Id, MovieTitle = movies[3].Title, Theater = "Cineplex Downtown", ShowDate = DateTime.Today.AddDays(1), ShowTime = "19:00", ScreenType = "Standard", Price = 10.99m }
            };
            await _context.MovieShowtimes.InsertManyAsync(showtimes);

            // Seed Bookings
            var bookings = new[]
            {
                new Booking { UserId = user1.Id, UserName = user1.FullName, Type = "Transport", ItemId = transports[0].Id, ItemName = transports[0].Name, Quantity = 2, TotalPrice = 179.98m, BookingDate = DateTime.Today.AddDays(5), Status = "Confirmed" },
                new Booking { UserId = user2.Id, UserName = user2.FullName, Type = "Package", ItemId = packages[0].Id, ItemName = packages[0].Name, Quantity = 2, TotalPrice = 5999.98m, BookingDate = DateTime.Today.AddDays(30), Status = "Confirmed" },
                new Booking { UserId = user3.Id, UserName = user3.FullName, Type = "Movie", ItemId = showtimes[0].Id, ItemName = showtimes[0].MovieTitle, Quantity = 3, TotalPrice = 47.97m, BookingDate = DateTime.Today.AddDays(1), Status = "Completed" }
            };
            await _context.Bookings.InsertManyAsync(bookings);
        }
    }
}
