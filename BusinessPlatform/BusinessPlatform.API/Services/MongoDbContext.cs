using BusinessPlatform.API.Models;
using MongoDB.Driver;

namespace BusinessPlatform.API.Services
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoDb");
            var databaseName = configuration["DatabaseName"];

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        // Collections
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Category> Categories => _database.GetCollection<Category>("Categories");
        public IMongoCollection<Product> Products => _database.GetCollection<Product>("Products");
        public IMongoCollection<ShoppingCartItem> ShoppingCartItems => _database.GetCollection<ShoppingCartItem>("ShoppingCartItems");
        public IMongoCollection<ShoppingOrder> ShoppingOrders => _database.GetCollection<ShoppingOrder>("ShoppingOrders");
        public IMongoCollection<AdCategory> AdCategories => _database.GetCollection<AdCategory>("AdCategories");
        public IMongoCollection<Advertisement> Advertisements => _database.GetCollection<Advertisement>("Advertisements");
        public IMongoCollection<AdResponse> AdResponses => _database.GetCollection<AdResponse>("AdResponses");
        public IMongoCollection<Job> Jobs => _database.GetCollection<Job>("Jobs");
        public IMongoCollection<JobApplication> JobApplications => _database.GetCollection<JobApplication>("JobApplications");
        public IMongoCollection<Candidate> Candidates => _database.GetCollection<Candidate>("Candidates");
        public IMongoCollection<Transport> Transports => _database.GetCollection<Transport>("Transports");
        public IMongoCollection<TravelPackage> TravelPackages => _database.GetCollection<TravelPackage>("TravelPackages");
        public IMongoCollection<Movie> Movies => _database.GetCollection<Movie>("Movies");
        public IMongoCollection<MovieShowtime> MovieShowtimes => _database.GetCollection<MovieShowtime>("MovieShowtimes");
        public IMongoCollection<Booking> Bookings => _database.GetCollection<Booking>("Bookings");
    }
}
