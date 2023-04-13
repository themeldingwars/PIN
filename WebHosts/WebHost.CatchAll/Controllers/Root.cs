using Microsoft.AspNetCore.Mvc;

namespace WebHost.CatchAll.Controllers;

[ApiController]
public class RootController : ControllerBase
{
    [Route("products.json")]
    [HttpGet]
    [Produces("application/json")]
    public object Products()
    {
        return new { test = true };
    }

    [Route("{*url}", Order = 999)]
    public IActionResult CatchAll()
    {
        return Ok();
    }
}
