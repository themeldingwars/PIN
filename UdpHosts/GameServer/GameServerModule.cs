using Autofac;
using Serilog;
using Shared.Common;

namespace GameServer;

public class GameServerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        RegisterTypes(builder);
        RegisterInstances(builder);
        base.Load(builder);
    }

    private static void RegisterTypes(ContainerBuilder builder)
    {
        builder.RegisterType<GameServerSettings>().SingleInstance();
        builder.RegisterType<GameServer>();
    }

    private static void RegisterInstances(ContainerBuilder builder)
    {
        builder.Register(ctx =>
                         {
                             var loggerConfig = new LoggerConfiguration()
                                                .ReadFrom.AppSettings()
                                                .WriteTo.Console(theme: SerilogTheme.Custom);

                             var settings = ctx.Resolve<GameServerSettings>();

                             if (settings.LogLevel.HasValue)
                             {
                                 loggerConfig = loggerConfig.MinimumLevel.Is(settings.LogLevel.Value);
                             }

                             return loggerConfig.CreateLogger();
                         }).As<ILogger>().SingleInstance();
    }
}