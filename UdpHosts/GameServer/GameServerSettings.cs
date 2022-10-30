using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    /// <summary>
    /// Holds the settings for the server
    /// </summary>
    public class GameServerSettings
    {
        /// <summary>
        /// The log level to use for the logger. Any messages below this level won't be printed to console.
        /// </summary>
        public LogEventLevel? LogLevel { get; set; }
    }
}
