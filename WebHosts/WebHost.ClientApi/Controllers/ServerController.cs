using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using WebHost.ClientApi.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebHost.ClientApi.Controllers {
	[ApiController]
	public class ServerController : ControllerBase {
		[Route("api/v1/server/list")]
		[HttpPost]
		public ServerList GetServerList() {
			return new ServerList();
		}
	}
}
