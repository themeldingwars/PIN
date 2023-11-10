using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Common;
using Shared.Web;
using WebHost.Market.Models;

namespace WebHost.Market.Controllers;

[ApiController]
public class MarketController : ControllerBase
{
    private readonly ILogger<MarketController> _logger;

    private Dictionary<string, ItemDisplayAttribute> _itemDisplayAttributes;
    private List<MarketCategory> _marketCategories;

    public MarketController(ILogger<MarketController> logger)
    {
        _logger = logger;

        LoadMarketData();
    }

    [HttpGet]
    [Route("")]
    public ActionResult Index()
    {
        return Ok();
    }

    [Version(1679)]
    [HttpGet]
    [Route("api/v1/item_display_attributes")]
    public ActionResult ItemDisplayAttributes()
    {
        return Ok(_itemDisplayAttributes);
    }
    
    [Version(1679)]
    [HttpGet]
    [Route("api/v1/market_categories")]
    public ActionResult MarketCategories()
    {
        return Ok(_marketCategories);
    }

    private void LoadMarketData()
    {
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = new SnakeCasePropertyNamingPolicy() };
        
        var attributes = System.IO.File.ReadAllText("Data/ItemDisplayAttributes.json");
        _itemDisplayAttributes = JsonSerializer.Deserialize<Dictionary<string, ItemDisplayAttribute>>(attributes, jsonOptions);
        
        var categories = System.IO.File.ReadAllText("Data/MarketCategories.json");
        _marketCategories = JsonSerializer.Deserialize<List<MarketCategory>>(categories, jsonOptions);
    }
}