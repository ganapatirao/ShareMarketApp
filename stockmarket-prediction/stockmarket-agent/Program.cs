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
builder.Services.AddOpenApi();

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
builder.Services.AddScoped<NSEOptionsService>();
builder.Services.AddScoped<KiteService>();
builder.Services.AddScoped<YahooFinanceOptionsService>();
builder.Services.AddScoped<FinnhubService>();
// builder.Services.AddHostedService<BackgroundSeedingService>();
// builder.Services.AddHostedService<DailyStockDataUpdateService>();

// Register HttpClient for external API calls
builder.Services.AddHttpClient<FinnhubService>();
builder.Services.AddHttpClient<OptionsDataService>();
builder.Services.AddHttpClient<NSEOptionsService>();
builder.Services.AddHttpClient<KiteService>();
builder.Services.AddHttpClient<YahooFinanceOptionsService>();

// Add Memory Cache
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseStaticFiles();
    app.MapOpenApi("openapi/v1.json");
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Serve static files from frontend build directory
app.UseStaticFiles();

// Map fallback to index.html for SPA routing (only in production)
if (!app.Environment.IsDevelopment())
{
    app.MapFallbackToFile("index.html");
}

app.Run();
