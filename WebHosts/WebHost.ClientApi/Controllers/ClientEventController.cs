using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Text.Json;
using WebHost.ClientApi.Models.ClientEvent;

namespace WebHost.ClientApi.Controllers;

[Route("api/v1")]
[ApiController]
public class ClientEventController : ControllerBase
{
    [Route("client_event")]
    [HttpPost]
    public object Post([FromBody] ClientEvent payload)
    {
        Log.Logger.Verbose(JsonSerializer.Serialize(payload));
        return new { };
    }
}