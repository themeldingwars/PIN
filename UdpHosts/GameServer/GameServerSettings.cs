using System;
using Aero.Protocol;
using Serilog.Core;
using Serilog.Events;

namespace GameServer;

/// <summary>
///     Holds the settings for the server`
/// </summary>
public class GameServerSettings
{
    [Flags]
    public enum LogOutput
    {
        None = 0,
        Console = 1,
        Seq = 2,
        File = 4,
    }

    /// <summary>
    ///     The log level to use for the logger. Any messages below this level won't be printed to console.
    /// </summary>
    public LogEventLevel? LogLevel { get; set; } = LogEventLevel.Debug;

    /// <summary>
    ///     Comma separated list of log outputs to use: Console, Files, Seq
    /// </summary>
    public LogOutput LogOutputs { get; set; } = LogOutput.Console;

    public LoggingLevelSwitch LevelSwitch { get; set; } = new();

    /// <summary>
    ///    UDP port the game server should be listening on
    /// </summary>
    public ushort Port { get; set; } = 25001;

    /// <summary>
    ///    Firefall client version this server instance serves. Used to resolve the network protocol.
    /// </summary>
    public string ClientVersion { get; set; } = "1962";

    /// <summary>
    ///    Firefall client environment this server instance serves. Used to resolve the network protocol.
    /// </summary>
    public string ClientEnvironment { get; set; } = "production";

    /// <summary>
    ///    Firefall client branch this server instance serves. Used to resolve the network protocol.
    /// </summary>
    public string ClientBranch { get; set; } = "prod";

    /// <summary>
    ///    GSS protocol version resolved from <see cref="ClientVersion" />, <see cref="ClientBranch" />, and <see cref="ClientEnvironment" /> at startup
    /// </summary>
    public GssVersion GssProtocolVersion { get; set; } = GssVersion.V67;

    /// <summary>
    ///    Matrix protocol version resolved from <see cref="ClientVersion" />, <see cref="ClientBranch" />, and <see cref="ClientEnvironment" /> at startup
    /// </summary>
    public MatrixVersion MatrixProtocolVersion { get; set; } = MatrixVersion.V26;

    /// <summary>
    ///    Address to use to connect to RIN.InternalAPI for GRPC. If the connection fails, GRPC will not be used.
    /// </summary>
    public string GrpcChannelAddress { get; set; } = "http://localhost:5201";

    /// <summary>
    ///    File path to "clientdb.sd2" located in the "db" folder of the Firefall installation.
    ///    Configured through GameServer.config.json (or App.config / GameServer.dll.config as a fallback).
    /// </summary>
    public string StaticDBPath { get; set; } = string.Empty;

    /// <summary>
    ///    Directory path to the "maps" folder of the Firefall installation.
    ///    Configured through GameServer.config.json (or App.config / GameServer.dll.config as a fallback).
    /// </summary>
    public string MapsPath { get; set; } = string.Empty;

    /// <summary>
    ///    Directory path to the "assetdb" folder of the Firefall installation.
    ///    Configured through GameServer.config.json (or App.config / GameServer.dll.config as a fallback).
    /// </summary>
    public string AssetDBPath { get; set; } = string.Empty;

    /// <summary>
    ///    Directory path for collision cache files (.bincache, .rbcache). The cache can be pregenerated with the CollisionGenerator tool.
    /// </summary>
    public string CachePath { get; set; } = string.Empty;

    /// <summary>
    ///    ZoneId to load
    /// </summary>
    public uint ZoneId { get; set; } = 448;

    /// <summary>
    ///    Enable loading zone collision data. Required for NPC ground snapping: without
    ///    the zone statics there is no terrain to ray cast against, so mobs stay at
    ///    their spawn height. Loading fails gracefully when <see cref="MapsPath" /> has
    ///    no zone file.
    /// </summary>
    public bool LoadMapsCollision { get; set; } = true;

    /// <summary>
    ///    Enable loading entities on zone startup
    /// </summary>
    public bool LoadZoneEntities { get; set; } = true;

    /// <summary>
    ///    Force reload zone from source files, bypassing cache.
    /// </summary>
    public bool ForceReloadZone { get; set; }

    /// <summary>
    ///    Batch multiple outgoing game messages into a single packet (up to the MTU budget).
    /// </summary>
    public bool BatchOutgoingPackets { get; set; } = true;
}