using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Shared.Common;
using Shared.Web.Config;
using System;
using System.Security.Authentication;
using WebHost = Microsoft.AspNetCore.WebHost;

namespace Shared.Web
{
    public abstract class BaseWebServer
    {
        public BaseWebServer(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public static IWebHost Build(Type serverType, IConfiguration configuration)
        {
            try
            {
                if (serverType.FullName == null)
                {
                    throw new ArgumentNullException(nameof(serverType.FullName));
                }

                Log.Information($"Starting web host {serverType.FullName}");
                return WebHost.CreateDefaultBuilder()
                              .UseConfiguration(configuration)
                              .UseSerilog()
                              .UseKestrel((builder, serverOpts) =>
                                          {
                                              serverOpts.ConfigureHttpsDefaults(opts =>
                                                                                {
                                                                                    opts.SslProtocols = SslProtocols.Tls | // Required by FF itself *sigh*, completely insecure
                                                                                                        SslProtocols.Tls12 |
                                                                                                        SslProtocols.Tls13;
                                                                                });
                                          })
                              .UseStartup(serverType)
                              .UseUrls(configuration.GetSection("Firefall").Get<Firefall>().WebHosts[serverType.FullName.Replace(".WebServer", "")].Urls.Split(";"))
                              .Build();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");

                return null;
            }
        }

        public static IWebHost Build<T>(IConfiguration configuration)
            where T : BaseWebServer
        {
            return Build(typeof(T), configuration);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            _ = services.AddControllers()
                        .AddJsonOptions(options =>
                                        {
                                            options.JsonSerializerOptions.PropertyNamingPolicy =
                                                new SnakeCasePropertyNamingPolicy();
                                        });

            _ = services.AddSingleton(Configuration.GetSection("Firefall").Get<Firefall>());

            ConfigureChildServices(services);
        }

        protected virtual void ConfigureChildServices(IServiceCollection services) { }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                _ = app.UseDeveloperExceptionPage();
            }

            _ = app.UseHttpsRedirection()
                   .UseSerilogRequestLogging()
                   .UseRouting()
                   .UseEndpoints(endpoints => { _ = endpoints.MapControllers(); });

            ConfigureChild(app, env);
        }

        protected virtual void ConfigureChild(IApplicationBuilder app, IWebHostEnvironment env) { }
    }
}