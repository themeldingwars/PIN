using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using Shared.Common;

namespace Replay {
	public class Startup {
		public IConfiguration Configuration { get; }

		public Startup( IConfiguration configuration ) {
			Configuration = configuration;
		}

		public void ConfigureServices( IServiceCollection services ) {
			services.AddControllers()
					.AddJsonOptions(options => {
						options.JsonSerializerOptions.PropertyNamingPolicy =
							new SnakeCasePropertyNamingPolicy();
					});
		}

		public void Configure( IApplicationBuilder app, IWebHostEnvironment env ) {
			if( env.IsDevelopment() )
				app.UseDeveloperExceptionPage();

			app.UseHttpsRedirection()
				.UseSerilogRequestLogging()
				.UseRouting()
				.UseAuthorization()
				.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
	}
}
