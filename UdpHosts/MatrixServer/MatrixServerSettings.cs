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
}