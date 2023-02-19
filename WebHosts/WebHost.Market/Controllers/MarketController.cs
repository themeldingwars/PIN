using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebHost.Market.Controllers;

[ApiController]
public class MarketController : ControllerBase
{
    private readonly ILogger<MarketController> _logger;

    public MarketController(ILogger<MarketController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Route("")]
    public ActionResult Index()
    {
        return Ok();
    }
}