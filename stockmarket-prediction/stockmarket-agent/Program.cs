using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using stockmarket_agent.Data;
using stockmarket_agent.Services;
using stockmarket_agent.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure DbContext for InMemory database
builder.Services.AddDbContext<StockMarketDbContext>(options =>
    options.UseInMemoryDatabase("StockMarketDb"));

// Configure MongoDB
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return client.GetDatabase(settings.DatabaseName);
});

// Register services
builder.Services.AddScoped<StockAnalysisService>();
builder.Services.AddScoped<StockDataFetcherService>();
builder.Services.AddScoped<MongoStockService>();
builder.Services.AddScoped<OptionsDataService>();
builder.Services.AddScoped<FinnhubService>();
builder.Services.AddHostedService<BackgroundSeedingService>();
// builder.Services.AddHostedService<DailyStockDataUpdateService>();

// Register HttpClient for external API calls
builder.Services.AddHttpClient<FinnhubService>();
builder.Services.AddHttpClient<OptionsDataService>();

// Add Memory Cache
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseStaticFiles();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Serve static files from frontend build directory
app.UseStaticFiles();

app.MapFallbackToFile("index.html");

app.Run();
