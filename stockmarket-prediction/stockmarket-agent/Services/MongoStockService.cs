using MongoDB.Driver;
using MongoDB.Bson;
using stockmarket_agent.Models;

namespace stockmarket_agent.Services
{
    public class MongoStockService
    {
        private readonly IMongoCollection<StockData> _stockCollection;
        private readonly IMongoDatabase _database;
        private readonly IMongoClient _client;
        private readonly StockDataFetcherService _stockDataFetcher;
        private readonly StockAnalysisService _stockAnalysisService;

        public MongoStockService(IMongoDatabase database, StockDataFetcherService stockDataFetcher, StockAnalysisService stockAnalysisService)
        {
            _client = database.Client;
            _database = database;
            _stockCollection = database.GetCollection<StockData>("stocks");
            _stockDataFetcher = stockDataFetcher;
            _stockAnalysisService = stockAnalysisService;
        }

        public async Task<List<StockData>> GetAllStocksAsync()
        {
            try
            {
                Console.WriteLine("Attempting to fetch all stocks from MongoDB...");
                var stocks = await _stockCollection.Find(_ => true).ToListAsync();
                Console.WriteLine($"Successfully fetched {stocks.Count} stocks from MongoDB");
                return stocks;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching stocks from MongoDB: {ex.Message}");
                return new List<StockData>();
            }
        }

        public async Task ClearStockCollectionAsync()
        {
            try
            {
                Console.WriteLine("Clearing stock collection...");
                await _stockCollection.DeleteManyAsync(_ => true);
                Console.WriteLine("Stock collection cleared successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing stock collection: {ex.Message}");
                throw;
            }
        }

        public async Task<StockData?> GetStockBySymbolAsync(string symbol)
        {
            try
            {
                return await _stockCollection.Find(s => s.Symbol == symbol).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching stock {symbol} from MongoDB: {ex.Message}");
                return null;
            }
        }

        public async Task<List<StockData>> GetStocksByFilterAsync(
            string? trend = null,
            string? sector = null,
            string? marketCapCategory = null,
            string? companyName = null,
            string? condition = null)
        {
            try
            {
                var filter = Builders<StockData>.Filter.Empty;
                var conditions = new List<FilterDefinition<StockData>>();

                if (!string.IsNullOrEmpty(trend))
                    conditions.Add(Builders<StockData>.Filter.Eq(s => s.Trend, trend));

                if (!string.IsNullOrEmpty(sector))
                    conditions.Add(Builders<StockData>.Filter.Eq(s => s.Sector, sector));

                if (!string.IsNullOrEmpty(marketCapCategory))
                    conditions.Add(Builders<StockData>.Filter.Eq(s => s.MarketCapCategory, marketCapCategory));

                if (!string.IsNullOrEmpty(companyName))
                {
                    conditions.Add(Builders<StockData>.Filter.Or(
                        Builders<StockData>.Filter.Regex(s => s.Symbol, new BsonRegularExpression($"^{companyName}", "i")),
                        Builders<StockData>.Filter.Regex(s => s.CompanyName, new BsonRegularExpression($"^{companyName}", "i"))
                    ));
                }

                if (conditions.Count > 0)
                {
                    if (condition?.ToUpper() == "OR")
                        filter = Builders<StockData>.Filter.Or(conditions);
                    else
                        filter = Builders<StockData>.Filter.And(conditions);
                }

                return await _stockCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching filtered stocks from MongoDB: {ex.Message}");
                return new List<StockData>();
            }
        }

        public async Task<List<string>> GetAllSymbolsAsync()
        {
            try
            {
                return await _stockCollection.Find(_ => true).Project(s => s.Symbol).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching symbols from MongoDB: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetDistinctSectorsAsync()
        {
            try
            {
                return await _stockCollection.Distinct(s => s.Sector, Builders<StockData>.Filter.Empty).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching sectors from MongoDB: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<long> GetTotalCountAsync()
        {
            try
            {
                return await _stockCollection.CountDocumentsAsync(_ => true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting stock count from MongoDB: {ex.Message}");
                return 0;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}");
                Console.WriteLine("MongoDB connection successful");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MongoDB connection failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SeedInitialDataAsync()
        {
            try
            {
                Console.WriteLine("Checking if MongoDB collection needs seeding...");
                var count = await _stockCollection.CountDocumentsAsync(_ => true);

                if (count > 0)
                {
                    Console.WriteLine($"MongoDB collection already has {count} documents. Clearing and reseeding...");
                    await _stockCollection.DeleteManyAsync(_ => true);
                    Console.WriteLine("Cleared existing documents from MongoDB");
                }

                Console.WriteLine("Fetching latest stock data from Yahoo Finance in batches to avoid rate limits...");
                var companies = stockmarket_agent.Data.DbSeeder.GetInitialCompanies();
                
                return await FetchAndStoreStockDataFromYahooAsync(companies);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding MongoDB: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateStockDataFromYahooAsync()
        {
            try
            {
                Console.WriteLine("Updating existing stock data from Yahoo Finance (without clearing)...");
                var companies = stockmarket_agent.Data.DbSeeder.GetInitialCompanies();
                
                return await FetchAndStoreStockDataFromYahooAsync(companies);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating MongoDB: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> FetchAndStoreStockDataFromYahooAsync(List<Models.Company> companies)
        {
            try
            {
                // Fetch data in batches to avoid rate limiting
                int batchSize = 10; // Fetch 10 stocks at a time to speed up
                int delayBetweenBatches = 5; // Wait 5 seconds between batches (retry logic will handle rate limits)
                int realDataCount = 0;
                int fallbackDataCount = 0;
                
                for (int i = 0; i < companies.Count; i += batchSize)
                {
                    var batch = companies.Skip(i).Take(batchSize).ToList();
                    Console.WriteLine($"Fetching batch {i / batchSize + 1} ({batch.Count} stocks)...");
                    
                    foreach (var company in batch)
                    {
                        try
                        {
                            var stockData = await _stockDataFetcher.FetchStockDataAsync(company.Symbol);
                            if (stockData != null)
                            {
                                stockData.CompanyName = company.Name;
                                stockData.Sector = company.Sector; // Use sector from company data (DbSeeder)
                                stockData.Industry = company.Industry;
                                stockData.MarketCapCategory = company.MarketCapCategory;
                                // PERatio is now fetched from Yahoo Finance API
                                
                                // Calculate additional fields
                                if (stockData.Week52High > 0)
                                {
                                    stockData.DiscountFromHigh = ((stockData.Week52High - stockData.Price) / stockData.Week52High) * 100;
                                }
                                
                                // Set trend based on price change
                                stockData.Trend = stockData.PriceChangePercentage >= 0 ? "Bullish" : "Bearish";
                                
                                // Apply buy and target price calculations using StockAnalysisService
                                stockData = _stockAnalysisService.AnalyzeStock(stockData);
                                
                                // Upsert data (update if exists, insert if not) to prevent duplicates
                                var filter = Builders<StockData>.Filter.Eq(s => s.Symbol, company.Symbol);
                                var update = Builders<StockData>.Update
                                    .SetOnInsert(s => s.Id, ObjectId.GenerateNewId().ToString())
                                    .Set(s => s.Symbol, stockData.Symbol)
                                    .Set(s => s.CompanyName, stockData.CompanyName)
                                    .Set(s => s.Sector, stockData.Sector)
                                    .Set(s => s.Industry, stockData.Industry)
                                    .Set(s => s.Price, stockData.Price)
                                    .Set(s => s.PreviousClose, stockData.PreviousClose)
                                    .Set(s => s.PriceChange, stockData.PriceChange)
                                    .Set(s => s.PriceChangePercentage, stockData.PriceChangePercentage)
                                    .Set(s => s.Week52High, stockData.Week52High)
                                    .Set(s => s.Week52Low, stockData.Week52Low)
                                    .Set(s => s.DiscountFromHigh, stockData.DiscountFromHigh)
                                    .Set(s => s.MarketCapCategory, stockData.MarketCapCategory)
                                    .Set(s => s.BuyPrice, stockData.BuyPrice)
                                    .Set(s => s.TargetPrice, stockData.TargetPrice)
                                    .Set(s => s.Trend, stockData.Trend)
                                    .Set(s => s.Volume, stockData.Volume)
                                    .Set(s => s.High, stockData.High)
                                    .Set(s => s.Low, stockData.Low)
                                    .Set(s => s.Open, stockData.Open)
                                    .Set(s => s.Close, stockData.Close)
                                    .Set(s => s.RSI, stockData.RSI)
                                    .Set(s => s.MACD, stockData.MACD)
                                    .Set(s => s.Volatility, stockData.Volatility)
                                    .Set(s => s.Momentum, stockData.Momentum)
                                    .Set(s => s.SupportLevel, stockData.SupportLevel)
                                    .Set(s => s.ResistanceLevel, stockData.ResistanceLevel)
                                    .Set(s => s.UpdatedAt, DateTime.UtcNow);
                                
                                var options = new UpdateOptions { IsUpsert = true };
                                await _stockCollection.UpdateOneAsync(filter, update, options);
                                Console.WriteLine($"  ✓ Upserted {company.Symbol} (₹{stockData.Price}) - Buy: ₹{stockData.BuyPrice}, Target: ₹{stockData.TargetPrice}");
                                realDataCount++;
                            }
                            else
                            {
                                Console.WriteLine($"  ✗ Failed to fetch data for {company.Symbol} - skipping (no fallback data)");
                                fallbackDataCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  ✗ Error fetching data for {company.Symbol}: {ex.Message} - skipping (no fallback data)");
                            fallbackDataCount++;
                        }
                        
                        // Small delay between individual requests (retry logic will handle rate limits)
                        await Task.Delay(500);
                    }
                    
                    // Wait between batches to avoid rate limiting
                    if (i + batchSize < companies.Count)
                    {
                        Console.WriteLine($"Waiting {delayBetweenBatches} seconds before next batch to avoid rate limiting...");
                        await Task.Delay(delayBetweenBatches * 1000);
                    }
                }

                Console.WriteLine($"Successfully processed {realDataCount + fallbackDataCount} stocks ({realDataCount} from Yahoo Finance, {fallbackDataCount} failed)");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching and storing stock data: {ex.Message}");
                return false;
            }
        }

        public async Task UpdateStockDataAsync(StockData stockData)
        {
            try
            {
                var filter = Builders<StockData>.Filter.Eq(s => s.Symbol, stockData.Symbol);
                var update = Builders<StockData>.Update
                    .Set(s => s.Price, stockData.Price)
                    .Set(s => s.Week52High, stockData.Week52High)
                    .Set(s => s.Week52Low, stockData.Week52Low)
                    .Set(s => s.DiscountFromHigh, stockData.DiscountFromHigh)
                    .Set(s => s.MarketCapCategory, stockData.MarketCapCategory)
                    .Set(s => s.BuyPrice, stockData.BuyPrice)
                    .Set(s => s.TargetPrice, stockData.TargetPrice)
                    .Set(s => s.Trend, stockData.Trend)
                    .Set(s => s.Volume, stockData.Volume)
                    .Set(s => s.Sector, stockData.Sector)
                    .Set(s => s.Industry, stockData.Industry)
                    .Set(s => s.CompanyName, stockData.CompanyName)
                    .Set(s => s.PreviousClose, stockData.PreviousClose)
                    .Set(s => s.PriceChange, stockData.PriceChange)
                    .Set(s => s.PriceChangePercentage, stockData.PriceChangePercentage)
                    .Set(s => s.High, stockData.High)
                    .Set(s => s.Low, stockData.Low)
                    .Set(s => s.Open, stockData.Open)
                    .Set(s => s.Close, stockData.Close)
                    .Set(s => s.MACD, stockData.MACD)
                    .Set(s => s.VolumeSMA, stockData.VolumeSMA)
                    .Set(s => s.SupportLevel, stockData.SupportLevel)
                    .Set(s => s.ResistanceLevel, stockData.ResistanceLevel)
                    .Set(s => s.Volatility, stockData.Volatility)
                    .Set(s => s.LastUpdated, stockData.LastUpdated)
                    .Set(s => s.UpdatedAt, DateTime.UtcNow);

                await _stockCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
                Console.WriteLine($"Updated stock data for {stockData.Symbol}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating stock data for {stockData.Symbol}: {ex.Message}");
                throw;
            }
        }

    }
}
