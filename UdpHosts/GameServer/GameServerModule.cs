using System;
using System.Configuration;
using Autofac;
using FauFau.Formats;
using GameServer.Physics.ZoneLoader;
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
        builder.RegisterType<StaticDB>().SingleInstance();
        builder.RegisterType<GameServer>();
    }

    private static void RegisterInstances(ContainerBuilder builder)
    {
        builder.Register(ctx =>
        {
            var settings = new GameServerSettings();

            if (ConfigurationManager.AppSettings["Port"] != null)
            {
                settings.Port = ushort.Parse(ConfigurationManager.AppSettings["Port"]);
            }

            if (ConfigurationManager.AppSettings["GrpcChannelAddress"] != null)
            {
                settings.GrpcChannelAddress = ConfigurationManager.AppSettings["GrpcChannelAddress"];
            }

            if (ConfigurationManager.AppSettings["StaticDBPath"] != null)
            {
                settings.StaticDBPath = ConfigurationManager.AppSettings["StaticDBPath"];
            }

            if (ConfigurationManager.AppSettings["MapsPath"] != null)
            {
                settings.MapsPath = ConfigurationManager.AppSettings["MapsPath"];

                if (ConfigurationManager.AppSettings["LoadMapsCollision"] != null)
                {
                    if (Boolean.TryParse(ConfigurationManager.AppSettings["LoadMapsCollision"], out bool value)) {
                        settings.LoadMapsCollision = value;
                    }
                    else
                    {
                        Console.WriteLine($"Cannot parse LoadMapsCollision setting value");
                    }
                }
            }

            return settings;
        })
        .As<GameServerSettings>().SingleInstance();

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
        })
        .As<ILogger>().SingleInstance();

        builder.Register(ctx =>
        {
            var settings = ctx.Resolve<GameServerSettings>();
            
            Console.WriteLine($"Opening SDB from {settings.StaticDBPath}");
            StaticDB sdb = new StaticDB();
            sdb.Read(settings.StaticDBPath);

            return sdb;
        })
        .As<StaticDB>().SingleInstance();
    }
}