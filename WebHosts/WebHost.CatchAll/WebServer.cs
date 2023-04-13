using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Web;

namespace WebHost.CatchAll;

public class WebServer : BaseWebServer
{
    public WebServer(IConfiguration configuration) : base(configuration) { }

    protected override void ConfigureChildServices(IServiceCollection services) { }
    protected override void ConfigureChild(IApplicationBuilder app, IWebHostEnvironment env) { }
}