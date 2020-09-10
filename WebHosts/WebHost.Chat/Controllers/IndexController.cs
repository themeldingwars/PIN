using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace WebHost.Chat.Controllers {
	[ApiController]
	public class IndexController : ControllerBase {
		[Route("")]
		[HttpGet]
		public async Task<object> Test() {
			return new { test = true };
		}
	}
}