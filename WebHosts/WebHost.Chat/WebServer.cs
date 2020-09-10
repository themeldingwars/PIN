using System;

using Microsoft.AspNetCore;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace WebHost.Chat {
	public class WebServer : Shared.Web.BaseWebServer {
		public WebServer( IConfiguration configuration ) : base(configuration) { }

		protected override void ConfigureChildServices( IServiceCollection services ) { }
		protected override void ConfigureChild( IApplicationBuilder app, IWebHostEnvironment env ) { }
	}
}
