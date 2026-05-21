namespace stockmarket_agent.Models
{
    public class OptionsData
    {
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public decimal SpotPrice { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercentage { get; set; }
        
        // Support levels (s1-s5)
        public decimal S1 { get; set; }
        public decimal S2 { get; set; }
        public decimal S3 { get; set; }
        public decimal S4 { get; set; }
        public decimal S5 { get; set; }
        
        // Resistance levels (r1-r5)    
        public decimal R1 { get; set; }
        public decimal R2 { get; set; }
        public decimal R3 { get; set; }
        public decimal R4 { get; set; }
        public decimal R5 { get; set; }

        // Additional fields   
        public string Trend { get; set; } = string.Empty;
        public decimal OpenInterest { get; set; }
        public decimal Volume { get; set; }
        public DateTime LastUpdated { get; set; }

        // Additional properties for OptionsDataService
        public decimal RegularPrice { get; set; }
        public decimal PreviousClose { get; set; }
        public decimal ChangePercentApp { get; set; }
        public string MarketCap { get; set; } = string.Empty;
        public double Support1 { get; set; }
        public double Support2 { get; set; }
        public double Support3 { get; set; }
        public double Support4 { get; set; }
        public double Support5 { get; set; }
        public double Resistance1 { get; set; }
        public double Resistance2 { get; set; }
        public double Resistance3 { get; set; }
        public double Resistance4 { get; set; }
        public double Resistance5 { get; set; }
    }
}
