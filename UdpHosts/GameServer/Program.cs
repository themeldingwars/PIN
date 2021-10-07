using Serilog;
using Shared.Common;
using Shared.Udp;
using System;
using System.Buffers.Binary;

namespace GameServer
{
    internal static class Program
    {
        public static ILogger Logger { get; private set; }

        private static void Main(string[] arguments)
        {
            Logger = new LoggerConfiguration()
                     .MinimumLevel.Verbose()
                     .WriteTo.Console(theme: SerilogTheme.Custom)
                     .CreateLogger();

            PacketServer.Logger = Logger;

            // TODO: Handle/allow args and configuration, add DI?
            Span<byte> ranSpan = stackalloc byte[8];
            new Random().NextBytes(ranSpan.Slice(2, 6));
            var serverId = BinaryPrimitives.ReadUInt64LittleEndian(ranSpan);
            var server = new GameServer(25001, serverId);
            server.Run();
        }
    }
}