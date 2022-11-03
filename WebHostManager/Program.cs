using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Shared.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebHost.Chat;

namespace WebHostManager
{
    internal static class Program
    {
        private static readonly IEnumerable<Type> HostTypes = new[]
                                                              {
                                                                  typeof(WebServer), typeof(WebHost.ClientApi.WebServer), typeof(WebHost.OperatorApi.WebServer), typeof(WebHost.InGameApi.WebServer), typeof(WebHost.Store.WebServer),
                                                                  typeof(WebHost.Replay.WebServer), typeof(WebHost.Market.WebServer), typeof(WebHost.WebAsset.WebServer)
                                                              };

        private static IConfiguration Configuration { get; } = new ConfigurationBuilder()
                                                               .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "config"))
                                                               .AddJsonFile("appsettings.json", false, true)
                                                               .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                                                               .AddEnvironmentVariables()
                                                               .Build();

        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                         .ReadFrom.Configuration(Configuration)
                         .CreateLogger();

            try
            {
                Log.Information("Starting Web Hosts");
                var ct = new CancellationToken();

                var hostsTasks = StartHosts(ct);

                Log.Information("All Web Hosts started, waiting for all to stop or break/kill signal. (Ctrl-c on Windows)");

                Task.WaitAll(hostsTasks.ToArray(), ct);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IEnumerable<Task> StartHosts(CancellationToken ct)
        {
            return HostTypes.Select(t => BaseWebServer.Build(t, Configuration).RunAsync(ct)).ToList();
        }
    }
}