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
				.WriteTo.Console(theme: Shared.Common.SerilogTheme.Custom)
				.CreateLogger();

			Shared.Udp.PacketServer.Logger = Logger;

			// TODO: Handle/allow args and configuration, add DI?
			var serverId = unchecked((uint)((new Random()).Next(1000,ushort.MaxValue)));
			var server = new GameServer(25001, serverId);
			server.Run();
		}
	}
}
