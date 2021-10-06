using Microsoft.AspNetCore.Mvc;
using WebHost.ClientApi.Models;
using WebHost.ClientApi.Models.Server;

namespace WebHost.ClientApi.Controllers
{
    [ApiController]
    public class ServerController : ControllerBase
    {
        [Route("api/v1/server/list")]
        [HttpPost]
        public ServerList GetServerList()
        {
            return new ServerList();
        }
    }
}