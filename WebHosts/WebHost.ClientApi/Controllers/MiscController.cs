using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using WebHost.ClientApi.Models;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace WebHost.ClientApi.Controllers {
	[ApiController]
	public class MiscController : ControllerBase {
		[Route("api/v1/login_alerts")]
		[HttpGet]
		public HttpResponseMessage GetLoginAlerts() {
			const string Html = @"<html>
<head>
<meta charset=""utf - 8"" />
<title>InGame</title>
</head>
<body>
<strong>Welcome to PIN</strong><br>
The Pirate Intelligence Network is here to serve you all the content you'd want
</body>
</html>";

			return new HttpResponseMessage {
				Content = new StringContent(Html, Encoding.UTF8, "text/html")
			};
		}



		[Route("api/v2/zone_settings")]
		[HttpGet]
		public async Task<object> ZoneSettings() {
			return new object[] { };
		}
	}
}