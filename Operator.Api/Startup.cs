using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Operator.Domain.Capability;
using Operator.Infrastructure.Capability;

using Serilog;

using Shared.Common;

namespace Operator.Api {
	public class Startup {
		public Startup( IConfiguration configuration ) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices( IServiceCollection services ) {
			services.AddControllers()
					.AddJsonOptions(options => {
						options.JsonSerializerOptions.PropertyNamingPolicy =
							new SnakeCasePropertyNamingPolicy();
					});

			services.AddScoped<ICapabilityRepository, CapabilityRepository>();
		}

		public void Configure( IApplicationBuilder app, IWebHostEnvironment env ) {
			if( env.IsDevelopment() )
				app.UseDeveloperExceptionPage();

			app.UseHttpsRedirection()
				.UseSerilogRequestLogging()
				.UseRouting()
				.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
	}
}