using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Web;
using WebHost.OperatorApi.Capability;

namespace WebHost.OperatorApi
{
    public class WebServer : BaseWebServer
    {
        public WebServer(IConfiguration configuration) : base(configuration) { }

        protected override void ConfigureChildServices(IServiceCollection services)
        {
            services.AddScoped<ICapabilityRepository, CapabilityRepository>();
        }

        protected override void ConfigureChild(IApplicationBuilder app, IWebHostEnvironment env) { }
    }
}