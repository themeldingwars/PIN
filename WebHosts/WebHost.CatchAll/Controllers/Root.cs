using System;
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
        return new Products() { Test = new Array[] { } };
    }

    [Route("{*url}", Order = 999)]
    public IActionResult CatchAll()
    {
        return Ok();
    }
}

public class Products
{
    public Array Test { get; set; }
}
