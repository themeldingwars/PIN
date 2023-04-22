namespace WebHost.Market.Models;

public class MarketCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int ParentId { get; set; }
}