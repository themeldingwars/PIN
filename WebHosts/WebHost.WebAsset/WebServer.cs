using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Shared.Web;
using System.IO;

namespace WebHost.WebAsset;

public class WebServer : BaseWebServer
{
    public WebServer(IConfiguration configuration) : base(configuration) { }

    protected override void ConfigureChildServices(IServiceCollection services) { }

    protected override void ConfigureChild(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "Assets")), RequestPath = string.Empty });
    }
}