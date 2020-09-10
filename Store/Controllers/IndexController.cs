using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

using Microsoft.AspNetCore.Mvc;

namespace Store.Controllers {
	[ApiController]
	public class IndexController : ControllerBase {
		[Route("")]
		[HttpGet]
		public HttpResponseMessage Test() {
			const string Html = @"<html>
<head>
<meta charset=""utf - 8"" />
<title>Test</title>
</head>
<body>
Home page test
</body>
</html>";

			return new HttpResponseMessage {
				Content = new StringContent(Html, Encoding.UTF8, "text/html")
			};
		}
	}
}