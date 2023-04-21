using CommandLine;
using Serilog.Events;

namespace MatrixServer;

/// <summary>
///     Declares the options which are supported on the CLI
/// </summary>
public class CliOptions
{
    [Option('l', "logLevel", Default = null, Required = false, HelpText = "The log level. Any messages below this level will not be printed to the console")]
    public LogEventLevel? LogLevel { get; set; }
}