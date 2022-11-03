using CommandLine;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using Shared.Common;
using Shared.Udp;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;

namespace GameServer
{
    internal static class Program
    {
        public static ILogger Logger { get; private set; }
        public static GameServerSettings GameServerSettings { get; private set; }

        private static void Main(string[] arguments)
        {

            ParseAndApplyCliOptions(arguments);

            var loggerConfig = new LoggerConfiguration()
                               .ReadFrom.AppSettings()
                               .WriteTo.Console(theme: SerilogTheme.Custom);

            if (GameServerSettings.LogLevel.HasValue)
            {
                loggerConfig = loggerConfig.MinimumLevel.Is(GameServerSettings.LogLevel.Value);
            }

            Logger = loggerConfig
                .CreateLogger();


            // TODO: add DI?

            PacketServer.Logger = Logger;

            
            Span<byte> ranSpan = stackalloc byte[8];
            new Random().NextBytes(ranSpan.Slice(2, 6));
            var serverId = BinaryPrimitives.ReadUInt64LittleEndian(ranSpan);
            var server = new GameServer(25001, serverId);
            server.Run();
        }

        private static void ParseAndApplyCliOptions(IEnumerable<string> arguments)
        {
            Parser.Default.ParseArguments<CliOptions>(arguments)
                  .WithParsed(RunOptions)
                  .WithNotParsed(HandleParseError);
        }

        private static void RunOptions(CliOptions options)
        {
            if (options.LogLevel != null)
            {
                GameServerSettings.LogLevel = options.LogLevel;
            }
        }

        private static void HandleParseError(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                Console.Error.WriteLine("Error when parsing options: " + error);
            }
        }
    }
}