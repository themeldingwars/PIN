using System;
using System.Configuration;
using Autofac;
using Serilog;
using Shared.Common;
using SDB = FauFau.Formats.StaticDB;

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
        builder.RegisterType<SDB>().SingleInstance();
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

            if (ConfigurationManager.AppSettings["ZoneId"] != null)
            {
                settings.ZoneId = uint.Parse(ConfigurationManager.AppSettings["ZoneId"]);
            }

            if (ConfigurationManager.AppSettings["MapsPath"] != null)
            {
                settings.MapsPath = ConfigurationManager.AppSettings["MapsPath"];

                if (ConfigurationManager.AppSettings["LoadMapsCollision"] != null)
                {
                    if (bool.TryParse(ConfigurationManager.AppSettings["LoadMapsCollision"], out bool value))
                    {
                        settings.LoadMapsCollision = value;
                    }
                    else
                    {
                        Console.WriteLine($"Cannot parse LoadMapsCollision setting value");
                    }
                }
            }

            if (ConfigurationManager.AppSettings["LoadZoneEntities"] != null)
            {
                if (bool.TryParse(ConfigurationManager.AppSettings["LoadZoneEntities"], out bool value))
                {
                    settings.LoadZoneEntities = value;
                }
                else
                {
                    Console.WriteLine($"Cannot parse LoadZoneEntities setting value");
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
            var sdb = new SDB();
            sdb.Read(settings.StaticDBPath);

            return sdb;
        })
        .As<SDB>().SingleInstance();
    }
}