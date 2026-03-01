using System;
using System.Configuration;
using Autofac;
using GameServer.Data.SDB;
using GameServer.Logging;
using Serilog;
using Serilog.Events;
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

            if (ConfigurationManager.AppSettings["serilog:minimum-level"] != null)
            {
                if (Enum.TryParse(ConfigurationManager.AppSettings["serilog:minimum-level"], out LogEventLevel result))
                {
                    settings.LogLevel = result;
                }
            }

            var logOutputs = ConfigurationManager.AppSettings["serilog:write-to"];
            if (logOutputs != null)
            {
                if (Enum.TryParse(logOutputs, true, out GameServerSettings.LogOutput outputs))
                {
                    settings.LogOutputs = outputs;
                }
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
            var settings = ctx.Resolve<GameServerSettings>();
            var initialLevel = settings.LogLevel ?? LogEventLevel.Debug;
            settings.LevelSwitch.MinimumLevel = initialLevel;
            var appSettings = ConfigurationManager.AppSettings;

            var loggerConfig = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .Enrich.FromLogContext()
                .Enrich.With<LogSystemEnricher>();

            if (settings.LogOutputs.HasFlag(GameServerSettings.LogOutput.Console))
            {
                var minLevelConsole = initialLevel;
                if (Enum.TryParse(appSettings["serilog:write-to:Console.restrictedToMinimumLevel"], out LogEventLevel result))
                {
                    minLevelConsole = result;
                }

                loggerConfig = loggerConfig.WriteTo.Console(theme: SerilogTheme.Custom, restrictedToMinimumLevel: minLevelConsole);
            }

            if (settings.LogOutputs.HasFlag(GameServerSettings.LogOutput.Seq))
            {
                var minLevelSeq = initialLevel;
                if (Enum.TryParse(appSettings["serilog:write-to:Seq.restrictedToMinimumLevel"],
                                  out LogEventLevel result))
                {
                    minLevelSeq = result;
                }

                var seqUrl = appSettings["serilog:write-to:Seq.serverUrl"] ?? "http://localhost:5341";
                loggerConfig = loggerConfig
                    .Enrich.With(new EntityIdEnricher())
                    .WriteTo.Seq(seqUrl, controlLevelSwitch: settings.LevelSwitch, restrictedToMinimumLevel: minLevelSeq);
            }

            if (settings.LogOutputs.HasFlag(GameServerSettings.LogOutput.File))
            {
                var minLevelFile = initialLevel;
                if (Enum.TryParse(appSettings["serilog:write-to:File.restrictedToMinimumLevel"], out LogEventLevel result))
                {
                    minLevelFile = result;
                }

                string LogTemplate(bool withSystem)
                    => $"[{{Timestamp:HH:mm:ss.fff}}] [{{Level:u3}}] {(withSystem ? "[{System}] " : string.Empty)}{{Message:lj}}{{NewLine}}{{Exception}}";

                loggerConfig = loggerConfig
                    .WriteTo.File(
                        "logs/master_.log",
                        outputTemplate: LogTemplate(true),
                        rollingInterval: RollingInterval.Day,
                        restrictedToMinimumLevel: minLevelFile)
                    .WriteTo.Map(
                        "System",
                        "General",
                        (system, wt) => wt.File(
                            $"logs/systems/{system}_.log",
                            outputTemplate: LogTemplate(false),
                            rollingInterval: RollingInterval.Day,
                            restrictedToMinimumLevel: minLevelFile));
            }

            const string SystemLevelPrefix = "serilog:system-level:";

            foreach (string key in appSettings.AllKeys)
            {
                if (key == null || !key.StartsWith(SystemLevelPrefix))
                {
                    continue;
                }

                var systemName = key[SystemLevelPrefix.Length..];
                var levelStr   = appSettings[key];
                if (!Enum.TryParse<LogEventLevel>(levelStr, out var systemLevel))
                {
                    continue;
                }

                loggerConfig = loggerConfig.Filter.ByExcluding(logEvent =>
                {
                    if (!logEvent.Properties.TryGetValue("System", out var val) ||
                        val is not ScalarValue { Value: string system })
                    {
                        return false;
                    }

                    return system == systemName && logEvent.Level < systemLevel;
                });
            }

            var logger = loggerConfig.CreateLogger();
            Log.Logger = logger;

            return logger;
        })
        .As<ILogger>().SingleInstance();

        builder.Register(ctx =>
        {
            var settings = ctx.Resolve<GameServerSettings>();

            Log.ForContext<SDBInterface>().Information("Opening SDB from {StaticDBPath}", settings.StaticDBPath);
            var sdb = new SDB();
            sdb.Read(settings.StaticDBPath);

            return sdb;
        })
        .As<SDB>().SingleInstance();
    }
}
