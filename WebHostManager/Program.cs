using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace WebHostManager {
	class Program {
		public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
							.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(),"config"))
							.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
							.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
							.AddEnvironmentVariables()
							.Build();

		private static IEnumerable<Type> hostTypes = new[] {
			typeof(WebHost.Chat.WebServer),
			typeof(WebHost.ClientApi.WebServer),
			typeof(WebHost.OperatorApi.WebServer),
			typeof(WebHost.InGameApi.WebServer),
		};

		static void Main( string[] args ) {
			Log.Logger = new LoggerConfiguration()
			   .ReadFrom.Configuration(Configuration)
			   .CreateLogger();

			try {
				Log.Information("Starting Web Hosts");
				var ct = new CancellationToken();

				var hostsTasks = StartHosts(ct);

				Log.Information("All Web Hosts started, waiting for all to stop or break/kill signal. (Ctrl-c on Windows)");

				Task.WaitAll(hostsTasks.ToArray(), ct);
			} catch( Exception ex ) {
				Log.Fatal(ex, "Host terminated unexpectedly");
			} finally {
				Log.CloseAndFlush();
			}
		}

		private static IEnumerable<Task> StartHosts( CancellationToken ct ) {
			var ret = new List<Task>();

			foreach( var t in hostTypes )
				ret.Add(Shared.Web.BaseWebServer.Build(t, Configuration).RunAsync(ct));

			return ret;
		}
	}
}
