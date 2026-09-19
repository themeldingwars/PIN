using System;
using System.Collections.Generic;
using System.Linq;
using Aero.Protocol;
using Autofac;
using GameServer.Logging;
using GameServer.StaticDB;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Shared.Common;
using SDB = FauFau.Formats.StaticDB;

namespace GameServer;

public class GameServerModule : Module
{
    private readonly IReadOnlyDictionary<string, string> _commandLineOverrides;

    public GameServerModule(IReadOnlyDictionary<string, string> commandLineOverrides)
    {
        _commandLineOverrides = commandLineOverrides;
    }

    protected override void Load(ContainerBuilder builder)
    {
        RegisterTypes(builder);
        RegisterInstances(builder, _commandLineOverrides);

        base.Load(builder);
    }

    private static void RegisterTypes(ContainerBuilder builder)
    {
        builder.RegisterType<GameServerSettings>().SingleInstance();
        builder.RegisterType<SDB>().SingleInstance();
        builder.RegisterType<GameServer>();
    }

    private static void RegisterInstances(ContainerBuilder builder, IReadOnlyDictionary<string, string> commandLineOverrides)
    {
        builder.Register(_ => new ConfigurationBuilder()
                              .SetBasePath(AppContext.BaseDirectory)
                              .AddJsonFile("appsettings.json", optional: false)
                              .AddJsonFile("appsettings.Local.json", optional: true)
                              .AddEnvironmentVariables()
                              .AddInMemoryCollection(commandLineOverrides)
                              .Build())
        .As<IConfiguration>().SingleInstance();

        builder.Register(ctx =>
        {
            var settings = new GameServerSettings();
            ctx.Resolve<IConfiguration>().GetSection("GameServer").Bind(settings);

            ResolveProtocolVersions(settings);

            return settings;
        })
        .As<GameServerSettings>().SingleInstance();

        builder.Register(ctx =>
        {
            var settings = ctx.Resolve<GameServerSettings>();
            var initialLevel = settings.LogLevel ?? LogEventLevel.Debug;
            settings.LevelSwitch.MinimumLevel = initialLevel;

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Is(initialLevel)
                .Enrich.FromLogContext()
                .Enrich.With<LogSystemEnricher>()
                .WriteToOpenTelemetryIfEnabled();

            if (settings.LogOutputs.HasFlag(GameServerSettings.LogOutput.Console))
            {
                loggerConfig = loggerConfig.WriteTo.Console(theme: SerilogTheme.Custom, restrictedToMinimumLevel: settings.Logging.Console ?? initialLevel);
            }

            if (settings.LogOutputs.HasFlag(GameServerSettings.LogOutput.Seq))
            {
                loggerConfig = loggerConfig
                    .Enrich.With(new EntityIdEnricher())
                    .WriteTo.Seq(
                        settings.Logging.SeqServerUrl,
                        controlLevelSwitch: settings.LevelSwitch,
                        restrictedToMinimumLevel: settings.Logging.Seq ?? initialLevel);
            }

            if (settings.LogOutputs.HasFlag(GameServerSettings.LogOutput.File))
            {
                var minLevelFile = settings.Logging.File ?? initialLevel;

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

            foreach (var (systemName, systemLevel) in settings.Logging.SystemLevels)
            {
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

    /// <summary>
    ///     Resolve the configured <see cref="GameServerSettings.ClientEnvironment" />,
    ///     <see cref="GameServerSettings.ClientBranch" /> and <see cref="GameServerSettings.ClientVersion" />
    ///     to the GSS/matrix protocol versions this server instance should speak.
    /// </summary>
    private static void ResolveProtocolVersions(GameServerSettings settings)
    {
        var environment = settings.ClientEnvironment.Trim();
        var branch = settings.ClientBranch.Trim();
        var clientVersion = settings.ClientVersion.Trim();

        var versionMatches = Patches.All
            .Where(p => string.Equals(p.Version, clientVersion, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(p.Version, $"{clientVersion}.0", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Patches.PatchInfo? patch = null;
        foreach (var p in versionMatches)
        {
            if (string.Equals(p.Environment, environment, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(p.Branch, branch, StringComparison.OrdinalIgnoreCase))
            {
                patch = p;
                break;
            }
        }

        if (patch == null)
        {
            if (versionMatches.Count > 0)
            {
                var where = string.Join(", ", versionMatches
                    .Select(p => $"{p.Environment}/{p.Branch}")
                    .Distinct()
                    .OrderBy(s => s, StringComparer.OrdinalIgnoreCase));
                throw new InvalidOperationException($"ClientVersion '{clientVersion}' was not found in environment '{environment}', branch '{branch}'. It exists in: {where}");
            }

            throw new InvalidOperationException($"Unknown ClientVersion '{clientVersion}' in environment '{environment}', branch '{branch}'. {KnownVersionsHint(environment, branch)}");
        }

        var info = patch.Value;
        if (!ProtocolVersions.TryGetGssVersion(info.GssProtocolVersion, out var gssVersion) ||
            !ProtocolVersions.TryGetMatrixVersion(info.MatrixProtocolVersion, out var matrixVersion))
        {
            throw new InvalidOperationException($"ClientVersion '{clientVersion}' maps to unknown protocol versions (GSS raw {info.GssProtocolVersion}, Matrix raw {info.MatrixProtocolVersion})");
        }

        settings.GssProtocolVersion = gssVersion;
        settings.MatrixProtocolVersion = matrixVersion;
    }

    /// <summary>
    ///     Build the "known versions" part of the unknown-version error: the latest versions of the
    ///     configured environment/branch, or the available environment/branch combinations when the
    ///     configured combination itself is unknown.
    /// </summary>
    private static string KnownVersionsHint(string environment, string branch)
    {
        var versions = Patches.All
            .Where(p => string.Equals(p.Environment, environment, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(p.Branch, branch, StringComparison.OrdinalIgnoreCase))
            .Select(p => p.Version)
            .Distinct()
            .ToList();

        if (versions.Count == 0)
        {
            var combos = string.Join(", ", Patches.All
                .GroupBy(p => $"{p.Environment}/{p.Branch}")
                .Select(g => g.Key)
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase));
            return $"Environment '{environment}', branch '{branch}' is unknown. Known combinations: {combos}";
        }

        return $"Known versions include: {string.Join(", ", versions.TakeLast(10))}";
    }
}
