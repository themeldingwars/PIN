using System;
using System.Buffers.Binary;

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
			Span<byte> ranSpan = stackalloc byte[8];
			(new Random()).NextBytes(ranSpan.Slice(2, 6));
			var serverId = BinaryPrimitives.ReadUInt64LittleEndian(ranSpan);
			var server = new GameServer(25001, serverId);
			server.Run();
		}
	}
}
