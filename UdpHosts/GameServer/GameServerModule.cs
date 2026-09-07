using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.Json;
using Aero.Protocol;
using Autofac;
using GameServer.Logging;
using GameServer.StaticDB;
using Serilog;
using Serilog.Events;
using Shared.Common;
using SDB = FauFau.Formats.StaticDB;

namespace GameServer;

public class GameServerModule : Module
{
    private const string DefaultConfigJson =
        "{\n" +
        "  \"StaticDBPath\": \"\",\n" +
        "  \"MapsPath\": \"\",\n" +
        "  \"AssetDBPath\": \"\",\n" +
        "  \"CachePath\": \"\"\n" +
        "}\n";

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

            if (ConfigurationManager.AppSettings["ClientVersion"] != null)
            {
                settings.ClientVersion = ConfigurationManager.AppSettings["ClientVersion"];
            }

            if (ConfigurationManager.AppSettings["ClientEnvironment"] != null)
            {
                settings.ClientEnvironment = ConfigurationManager.AppSettings["ClientEnvironment"];
            }

            if (ConfigurationManager.AppSettings["ClientBranch"] != null)
            {
                settings.ClientBranch = ConfigurationManager.AppSettings["ClientBranch"];
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
                        Log.Error($"Cannot parse LoadMapsCollision setting value");
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
                    Log.Error($"Cannot parse LoadZoneEntities setting value");
                }
            }

            if (ConfigurationManager.AppSettings["AssetDBPath"] != null)
            {
                settings.AssetDBPath = ConfigurationManager.AppSettings["AssetDBPath"];
            }

            if (ConfigurationManager.AppSettings["CachePath"] != null)
            {
                settings.CachePath = ConfigurationManager.AppSettings["CachePath"];
            }

            if (ConfigurationManager.AppSettings["ForceReloadZone"] != null)
            {
                if (bool.TryParse(ConfigurationManager.AppSettings["ForceReloadZone"], out bool forceReload))
                {
                    settings.ForceReloadZone = forceReload;
                }
            }

            if (ConfigurationManager.AppSettings["BatchOutgoingPackets"] != null)
            {
                if (bool.TryParse(ConfigurationManager.AppSettings["BatchOutgoingPackets"], out bool batchOutgoingPackets))
                {
                    settings.BatchOutgoingPackets = batchOutgoingPackets;
                }
                else
                {
                    Log.Error($"Cannot parse BatchOutgoingPackets setting value");
                }
            }

            ApplyPathSettingsFromFile(settings);

            ResolveProtocolVersions(settings);

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
                if (key == null || !key.StartsWith(SystemLevelPrefix, StringComparison.InvariantCulture))
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

            if (string.IsNullOrWhiteSpace(settings.StaticDBPath))
            {
                throw new InvalidOperationException(
                    "StaticDBPath is not configured and no Firefall installation could be detected. Edit GameServer.config.json next to GameServer.exe and set \"StaticDBPath\" to the full path of clientdb.sd2, or set PIN_FIREFALL_PATH to your Firefall install directory.");
            }

            if (!File.Exists(settings.StaticDBPath))
            {
                throw new FileNotFoundException(
                    $"StaticDB file not found at '{settings.StaticDBPath}'. Set \"StaticDBPath\" in GameServer.config.json to the correct clientdb.sd2 path.",
                    settings.StaticDBPath);
            }

            Log.ForContext<SDBInterface>().Information("Opening SDB from {StaticDBPath}", settings.StaticDBPath);
            var sdb = new SDB();
            sdb.Read(settings.StaticDBPath);

            return sdb;
        })
        .As<SDB>().SingleInstance();
    }

    /// <summary>
    ///     Applies installation paths from GameServer.config.json.
    ///     The file is optional; when present its non-empty path values override the legacy App.config values.
    /// </summary>
    private static void ApplyPathSettingsFromFile(GameServerSettings settings)
    {
        var configPath = GetOrCreateConfigFile();
        if (configPath == null)
        {
            return;
        }

        GameServerConfigFile config;
        try
        {
            var json = File.ReadAllText(configPath);
            config = JsonSerializer.Deserialize<GameServerConfigFile>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            throw new InvalidOperationException(
                $"Could not read configuration file '{configPath}'. Fix or remove it and try again.",
                ex);
        }

        if (config == null)
        {
            throw new InvalidOperationException(
                $"Configuration file '{configPath}' does not contain valid JSON.");
        }

        AutoFillMissingPaths(configPath, config, settings);

        // Only non-empty values from the config file override settings; an empty string must
        // never replace a value that was already picked up from the legacy App.config.
        if (!string.IsNullOrWhiteSpace(config.StaticDBPath))
        {
            settings.StaticDBPath = config.StaticDBPath;
        }

        if (!string.IsNullOrWhiteSpace(config.MapsPath))
        {
            settings.MapsPath = config.MapsPath;
        }

        if (!string.IsNullOrWhiteSpace(config.AssetDBPath))
        {
            settings.AssetDBPath = config.AssetDBPath;
        }

        if (!string.IsNullOrWhiteSpace(config.CachePath))
        {
            settings.CachePath = config.CachePath;
        }
    }

    /// <summary>
    ///     Detect a Firefall installation and write the resolved paths back into the
    ///     configuration file when any of them are still empty. Values already provided by
    ///     the config file or the legacy App.config are never overwritten.
    /// </summary>
    private static void AutoFillMissingPaths(string configPath, GameServerConfigFile config, GameServerSettings settings)
    {
        var needStaticDbPath = string.IsNullOrWhiteSpace(config.StaticDBPath) &&
                               string.IsNullOrWhiteSpace(settings.StaticDBPath);
        var needMapsPath = string.IsNullOrWhiteSpace(config.MapsPath) &&
                           string.IsNullOrWhiteSpace(settings.MapsPath);
        var needAssetDbPath = string.IsNullOrWhiteSpace(config.AssetDBPath) &&
                              string.IsNullOrWhiteSpace(settings.AssetDBPath);

        if (!needStaticDbPath && !needMapsPath && !needAssetDbPath)
        {
            return;
        }

        var detected = FirefallInstallLocator.Locate();
        if (detected == null)
        {
            if (needStaticDbPath)
            {
                Log.Warning("Could not auto-detect a Firefall installation. Set StaticDBPath in {ConfigPath}.", configPath);
            }

            return;
        }

        if (needStaticDbPath)
        {
            config.StaticDBPath = detected.StaticDBPath;
        }

        if (needMapsPath)
        {
            config.MapsPath = detected.MapsPath;
        }

        if (needAssetDbPath)
        {
            config.AssetDBPath = detected.AssetDBPath;
        }

        Log.Information("Auto-detected Firefall at {Root}. Writing paths to {ConfigPath}.", detected.Root, configPath);
        WriteConfigFile(configPath, config);
    }

    /// <summary>
    ///     Locate GameServer.config.json in the working directory or next to the executable;
    ///     create it from the template when it does not exist yet.
    /// </summary>
    private static string GetOrCreateConfigFile()
    {
        var existing = FindConfigFile();
        if (existing != null)
        {
            return existing;
        }

        foreach (var directory in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() }.Distinct())
        {
            try
            {
                var path = Path.Combine(directory, "GameServer.config.json");
                if (!File.Exists(path))
                {
                    File.WriteAllText(path, DefaultConfigJson);
                    Log.Information("Created GameServer.config.json at {Path}", path);
                }

                return path;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                // The directory is not usable; try the next candidate.
            }
        }

        return null;
    }

    /// <summary>
    ///     Persist the configuration file, ignoring failures that should not prevent startup.
    /// </summary>
    private static void WriteConfigFile(string configPath, GameServerConfigFile config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            Log.Warning(ex, "Could not write auto-detected paths to {ConfigPath}", configPath);
        }
    }

    /// <summary>
    ///     Locate GameServer.config.json in the working directory or next to the executable.
    /// </summary>
    private static string FindConfigFile()
    {
        return new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory }
            .Select(directory => Path.Combine(directory, "GameServer.config.json"))
            .Distinct()
            .FirstOrDefault(File.Exists);
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
