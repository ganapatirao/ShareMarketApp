namespace stockmarket_agent.Settings
{
    public class MongoDbSettings
    {
        public const string SectionName = "MongoDbSettings";
        public string ConnectionString { get; set; } = "mongodb://localhost:27017";
        public string DatabaseName { get; set; } = "StockMarketDb";
    }
}
