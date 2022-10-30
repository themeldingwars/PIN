using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebHost.ClientApi.Models.ClientEvent;

namespace WebHost.ClientApi.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class ClientEventController : ControllerBase
    {
        [Route("client_event")]
        [HttpPost]
        public object Post([FromBody]ClientEvent payload)
        {
            Log.Logger.Information(payload.ToString());
            return new { };
        }
    }
}
