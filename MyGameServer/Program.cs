using System;

using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace MyGameServer {
	class Program {
		public static ILogger Logger { get; protected set; }
		static void Main( string[] args ) {
			Logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo.Console(theme: ServerShared.SerilogTheme.Custom)
				.CreateLogger();

			ServerShared.PacketServer.Logger = Logger;

			// TODO: Handle/allow args and configuration
			var server = new GameServer(25001);
			server.Run();
		}
	}
}
