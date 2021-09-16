using Serilog;
using Shared.Common;
using Shared.Udp;

namespace MyMatrixServer
{
    internal class Program
    {
        public static ILogger Logger { get; protected set; }

        private static void Main(string[] args)
        {
            Logger = new LoggerConfiguration()
                     .MinimumLevel.Verbose()
                     .WriteTo.Console(theme: SerilogTheme.Custom)
                     .CreateLogger();

            PacketServer.Logger = Logger;

            // TODO: Handle/allow args and configuration
            var server = new MatrixServer(25000);
            server.Run();
        }
    }
}