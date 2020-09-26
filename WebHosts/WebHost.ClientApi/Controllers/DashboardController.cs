using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebHost.ClientApi.Controllers {
	[ApiController]
	public class DashboardController : ControllerBase {
		[Route("api/v4/dashboard/conductor-assets")]
		[HttpGet]
		public object ConductorAssets() {
			return new { };
		}

		[Route("api/v4/dashboard/conductor-events")]
		[HttpGet]
		public object ConductorEvents() {
			return new { };
		}
	}
}
