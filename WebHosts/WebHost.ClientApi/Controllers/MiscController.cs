using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;

namespace WebHost.ClientApi.Controllers;

[ApiController]
public class MiscController : ControllerBase
{
    [Route("api/v1/login_alerts")]
    [HttpGet]
    public HttpResponseMessage GetLoginAlerts()
    {
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

        return new HttpResponseMessage { Content = new StringContent(Html, Encoding.UTF8, "text/html") };
    }


    [Route("api/v2/zone_settings")]
    [HttpGet]
    public object ZoneSettings()
    {
        return new object[] { };
    }
}