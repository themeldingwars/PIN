using Microsoft.AspNetCore.Mvc;
using System;
using WebHost.ClientApi.Models.Server;

namespace WebHost.ClientApi.Controllers;

[ApiController]
public class ServerController : ControllerBase
{
    [Route("api/v1/server/list")]
    [HttpPost]
    [Produces("application/json")]
    public ServerList GetServerList()
    {
        ServerList ServerList = new ServerList()
        {
            ZoneList = Array.Empty<long>()
        };

        return ServerList;
    }
}