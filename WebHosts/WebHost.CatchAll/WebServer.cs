using Microsoft.Extensions.Configuration;
using Shared.Web;

namespace WebHost.CatchAll;

public class WebServer : BaseWebServer
{
    public WebServer(IConfiguration configuration) : base(configuration) { }
}