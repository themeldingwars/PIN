using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebHost.ClientApi {
	public class WebServer : Shared.Web.BaseWebServer {
		public WebServer( IConfiguration configuration ) : base(configuration) { }

		protected override void ConfigureChildServices( IServiceCollection services ) { }
		protected override void ConfigureChild( IApplicationBuilder app, IWebHostEnvironment env ) { }
	}
}
