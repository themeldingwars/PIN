using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebHost.CatchAll {
	public class WebServer : Shared.Web.BaseWebServer {
		public WebServer( IConfiguration configuration ) : base(configuration) { }
	}
}
