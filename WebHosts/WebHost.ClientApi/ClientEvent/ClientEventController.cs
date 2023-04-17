using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Text.Json;

namespace WebHost.ClientApi.ClientEvent;

[Route("api/v1")]
[ApiController]
public class ClientEventController : ControllerBase
{
    [Route("client_event")]
    [HttpPost]
    public object Post([FromBody] Models.ClientEvent payload)
    {
        Log.Logger.Verbose(JsonSerializer.Serialize(payload));
        return new { };
    }
}