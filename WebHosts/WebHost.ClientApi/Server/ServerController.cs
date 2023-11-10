using System;
using Microsoft.AspNetCore.Mvc;
using WebHost.ClientApi.Server.Models;

namespace WebHost.ClientApi.Server;

[ApiController]
public class ServerController : ControllerBase
{
    [Route("api/v1/server/list")]
    [HttpPost]
    [Produces("application/json")]
    public ServerList GetServerList()
    {
        var serverList = new ServerList { ZoneList = Array.Empty<long>() };

        return serverList;
    }
}