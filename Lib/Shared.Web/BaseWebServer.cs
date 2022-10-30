using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Shared.Common;
using Shared.Web.Config;
using System;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Text;
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
            services.AddSwaggerGen();
            _ = services.AddControllers()
                        .AddJsonOptions(options =>
                                        {
                                            options.JsonSerializerOptions.PropertyNamingPolicy =
                                                new SnakeCasePropertyNamingPolicy();
                                        });

            _ = services.AddSingleton(Configuration.GetSection("Firefall").Get<Firefall>());

            ConfigureChildServices(services);
        }

        protected virtual void ConfigureChildServices(IServiceCollection services)
        {
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseSwagger();
            app.UseSwaggerUI(c =>
                             {
                                 c.SwaggerEndpoint("/swagger/v1/swagger.json", "Firefall API");
                             });

            // log requests which produce 404 responses so we can fill in the missing endpoints
            app.Use(async (context, next) =>
                    {
                        context.Request.EnableBuffering();
                        await next();
                        if (context.Response.StatusCode == (int)HttpStatusCode.NotFound)
                        {
                            var req = context.Request;
                            using var streamReader =
                                new StreamReader(req.Body, Encoding.UTF8, true, 1024, true);
                            Log.Error($"NotFound: {req.GetEncodedUrl()} ;; {req.Method} ;; {await streamReader.ReadToEndAsync()}");
                            req.Body.Position = 0;
                            await next();
                        }
                    });

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