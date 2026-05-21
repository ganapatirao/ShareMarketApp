namespace stockmarket_agent.Models;

public class Company
{
    public int Id { get; set; }
    public string Symbol { get; set; }
    public string Name { get; set; }
    public string Sector { get; set; }
    public string Industry { get; set; }
    public string MarketCapCategory { get; set; }
    public double BasePrice { get; set; }
    public double PERatio { get; set; }
    public bool IsActive { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
}
