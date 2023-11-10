using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Serilog;

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