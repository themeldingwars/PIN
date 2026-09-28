using System;
using Autofac;
using Microsoft.Extensions.Configuration;
using Serilog;
using Shared.Common;

namespace MatrixServer;

public class MatrixServerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        RegisterTypes(builder);
        RegisterInstances(builder);
        base.Load(builder);
    }

    private static void RegisterTypes(ContainerBuilder builder)
    {
        builder.RegisterType<MatrixServer>();
    }

    private static void RegisterInstances(ContainerBuilder builder)
    {
        builder.Register(_ => new ConfigurationBuilder()
                              .SetBasePath(AppContext.BaseDirectory)
                              .AddJsonFile("appsettings.json", optional: false)
                              .AddJsonFile("appsettings.Local.json", optional: true)
                              .AddEnvironmentVariables()
                              .Build())
               .As<IConfiguration>().SingleInstance();

        builder.Register(ctx =>
                         {
                             var settings = new MatrixServerSettings();
                             ctx.Resolve<IConfiguration>().GetSection("MatrixServer").Bind(settings);

                             return settings;
                         }).As<MatrixServerSettings>().SingleInstance();

        builder.Register(ctx =>
                         {
                             var loggerConfig = new LoggerConfiguration()
                                                .WriteTo.Console(theme: SerilogTheme.Custom);

                             var settings = ctx.Resolve<MatrixServerSettings>();

                             if (settings.LogLevel.HasValue)
                             {
                                 loggerConfig = loggerConfig.MinimumLevel.Is(settings.LogLevel.Value);
                             }

                             return loggerConfig.CreateLogger();
                         }).As<ILogger>().SingleInstance();
    }
}