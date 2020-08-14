using System;

using Serilog;
using Serilog.Events;

namespace MyMatrixServer {
	class Program {
		public static ILogger Logger { get; protected set; }
		static void Main( string[] args ) {
			Logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo.Console()
				.CreateLogger();

			ServerShared.PacketServer.Logger = Logger;

			// TODO: Handle/allow args and configuration
			var server = new MatrixServer(25000);
			server.Run();
		}
	}
}
