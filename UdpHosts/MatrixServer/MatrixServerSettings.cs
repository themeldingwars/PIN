using Serilog.Events;

namespace MatrixServer;

/// <summary>
///     Holds the settings for the server
/// </summary>
public class MatrixServerSettings
{
    /// <summary>
    ///     The log level to use for the logger. Any messages below this level won't be printed to console.
    /// </summary>
    public LogEventLevel? LogLevel { get; set; }

    /// <summary>
    ///     UDP port the game server should be listening on
    /// </summary>
    public ushort Port { get; set; } = 25000;

    /// <summary>
    ///     UDP port of the game server that clients get handed over to
    /// </summary>
    public ushort GameServerPort { get; set; } = 25001;

    /// <summary>
    ///     Optional URL for an HTTP health endpoint, e.g. "http://localhost:25080/"
    /// </summary>
    public string HealthUrl { get; set; } = string.Empty;
}