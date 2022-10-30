using CommandLine;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    /// <summary>
    /// Declares the options which are supported on the CLI
    /// </summary>
    public class CliOptions
    {
        [Option('l', "logLevel", Default = null, Required = false, HelpText = "The log level. Any messages below this level will not be printed to the console")]
        public LogEventLevel? LogLevel { get; set; }
    }
}
