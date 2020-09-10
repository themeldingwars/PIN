using System;

using Microsoft.AspNetCore;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Shared.Web {
	public abstract class BaseWebServer {
		public static IWebHost Build( Type serverType, IConfiguration configuration) {
			try {
				Log.Information("Starting web host {ProjectName}", serverType.FullName);
				return WebHost.CreateDefaultBuilder()
					   .UseConfiguration(configuration)
					   .UseSerilog()
					   .UseStartup(serverType)
					   .UseUrls(configuration.GetSection("Firefall").Get<Config.Firefall>().WebHosts[serverType.FullName.Replace(".WebServer","")].Urls.Split(";"))
					   .Build();
			} catch( Exception ex ) {
				Log.Fatal(ex, "Host terminated unexpectedly");

				return null;
			}
		}

		public static IWebHost Build<T>( IConfiguration configuration) where T : BaseWebServer {
			return Build(typeof(T), configuration);
		}

		public IConfiguration Configuration { get; private set; }

		public BaseWebServer( IConfiguration configuration ) {
			Configuration = configuration;
		}

		public void ConfigureServices( IServiceCollection services ) {
			services.AddControllers()
					.AddJsonOptions(options => {
						options.JsonSerializerOptions.PropertyNamingPolicy =
							new Common.SnakeCasePropertyNamingPolicy();
					});

			services.AddSingleton(Configuration.GetSection("Firefall").Get<Config.Firefall>());

			ConfigureChildServices(services);
		}

		protected virtual void ConfigureChildServices( IServiceCollection services ) { }

		public void Configure( IApplicationBuilder app, IWebHostEnvironment env ) {
			if( env.IsDevelopment() )
				app.UseDeveloperExceptionPage();

			app.UseHttpsRedirection()
				.UseSerilogRequestLogging()
				.UseRouting()
				.UseEndpoints(endpoints => { endpoints.MapControllers(); });

			ConfigureChild(app, env);
		}

		protected virtual void ConfigureChild( IApplicationBuilder app, IWebHostEnvironment env ) { }
	}
}
