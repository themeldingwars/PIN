using System;
using System.IO;
using System.Net;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace ClientApi.Api {
	public class Program {
		public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
			.AddEnvironmentVariables()
			.Build();

		public static int Main( string[] args ) {
			Log.Logger = new LoggerConfiguration()
			   .ReadFrom.Configuration(Configuration)
			   .CreateLogger();

			try {
				Log.Information("Starting web host {ProjectName}", typeof(Program).Namespace);
				CreateHostBuilder(args).Build()
									   .Run();
				return 0;
			} catch( Exception ex ) {
				Log.Fatal(ex, "Host terminated unexpectedly");
				return 1;
			} finally {
				Log.CloseAndFlush();
			}
		}

		public static IHostBuilder CreateHostBuilder( string[] args ) {
			return Host.CreateDefaultBuilder(args)
					   .UseSerilog()
					   .ConfigureWebHostDefaults(webBuilder => {
						   webBuilder.UseStartup<Startup>();
					   });
		}
	}
}