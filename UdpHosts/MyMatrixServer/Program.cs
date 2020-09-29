using System;

using Serilog;
using Serilog.Events;

namespace MyMatrixServer {
	class Program {
		public static ILogger Logger { get; protected set; }
		static void Main( string[] args ) {
			Logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo.Console(theme: Shared.Common.SerilogTheme.Custom)
				.CreateLogger();

			Shared.Udp.PacketServer.Logger = Logger;

			// TODO: Handle/allow args and configuration
			var server = new MatrixServer(25000);
			server.Run();
		}
	}
}
