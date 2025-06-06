﻿using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace WebHost.InGameApi.Controllers;

[Route("panelmanager")]
[ApiController]
public class PanelManagerController : ControllerBase
{
    [HttpGet]
    public HttpResponseMessage GetPanelManager()
    {
        return new HttpResponseMessage
               {
                   Content = new StringContent(@"<!DOCTYPE html>
<html>
<head>
<meta charset=""utf - 8"" />
<title>InGame</title>
<link href = ""/assets/in-game/application-e6b9c1f0e3d1ae901d00a2626efed74a.css"" media=""screen"" rel=""stylesheet"" type=""text/css"" />

<meta content = ""authenticity_token"" name=""csrf-param"" />
<meta content = ""90mwbRLJVGanE69AjhcRCQgzcGQnIlEs6SdY04UsXD8="" name=""csrf-token"" />

</head>
<body>
Hello World
</body>
</html>",
                                               Encoding.UTF8, 
                                               "text/html")
               };
    }
}