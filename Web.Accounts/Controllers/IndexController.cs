using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

using Microsoft.AspNetCore.Mvc;

namespace Web.Accounts.Controllers {
	[ApiController]
	public class IndexController : ControllerBase {
		[Route("")]
		[HttpGet]
		public object Test() {
			return new { error = false };
		}
	}
}