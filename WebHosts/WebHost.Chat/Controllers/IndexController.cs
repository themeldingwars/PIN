using Microsoft.AspNetCore.Mvc;

namespace WebHost.Chat.Controllers;

[ApiController]
public class IndexController : ControllerBase
{
    [Route("")]
    [HttpGet]
    public object Test()
    {
        return new { test = true };
    }
}