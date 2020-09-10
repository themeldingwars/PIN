using Microsoft.AspNetCore.Builder;

using Serilog;

namespace StaticContent.Web {
	public class Startup {
		public void Configure( IApplicationBuilder app ) {
			
			app.UseSerilogRequestLogging()
			   .UseStaticFiles();
		}
	}
}