using CommandLine;
using Serilog;
using Shared.Common;
using Shared.Udp;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace GameServer
{
    internal static class Program
    {
        public static ILogger Logger { get; private set; }
        public static GameServerSettings GameServerSettings { get; private set; }

        private static void Main(string[] arguments)
        {
            // TODO: add DI?
            LoadAppSettings();
            ParseAndApplyCliOptions(arguments);
            SetupLogger();


            var serverId = GenerateServerId();
            var server = new GameServer(25001, serverId);
            server.Run();
        }

        /// <summary>
        /// Set the logger up
        /// </summary>
        private static void SetupLogger()
        {
            var loggerConfig = new LoggerConfiguration()
                               .ReadFrom.AppSettings()
                               .WriteTo.Console(theme: SerilogTheme.Custom);

            if (GameServerSettings.LogLevel.HasValue)
            {
                loggerConfig = loggerConfig.MinimumLevel.Is(GameServerSettings.LogLevel.Value);
            }

            Logger = loggerConfig
                .CreateLogger();

            PacketServer.Logger = Logger;
        }

        /// <summary>
        /// Load the settings from App.config
        /// </summary>
        private static void LoadAppSettings()
        {
            GameServerSettings = new GameServerSettings();
            // no settings as of yet, expand this function should the need arise
        }

        /// <summary>
        /// Generate the Server Id
        /// ToDo: Incorporate the Sql Node Number as per https://gist.github.com/SilentCLD/881839a9f45578f1618db012fc789a71
        /// </summary>
        /// <returns></returns>
        private static ulong GenerateServerId()
        {
            Span<byte> ranSpan = stackalloc byte[8];
            new Random().NextBytes(ranSpan.Slice(2, 6));
            var ret = BinaryPrimitives.ReadUInt64LittleEndian(ranSpan);
            return ret;
        }

        /// <summary>
        /// Parse the options passed via the command line and overwrite settings from the config
        /// </summary>
        /// <param name="arguments"></param>
        private static void ParseAndApplyCliOptions(IEnumerable<string> arguments)
        {
            Parser.Default.ParseArguments<CliOptions>(arguments)
                  .WithParsed(RunOptions)
                  .WithNotParsed(HandleParseError);
        }

        /// <summary>
        /// Handle the parsed options, essentially overwriting already present settings loaded from App.config
        /// </summary>
        /// <param name="options"></param>
        private static void RunOptions(CliOptions options)
        {
            if (options.LogLevel != null)
            {
                GameServerSettings.LogLevel = options.LogLevel;
            }
        }

        /// <summary>
        /// If errors occur during the parsing of CLI options, they should be handled here
        /// </summary>
        /// <param name="errors"></param>
        private static void HandleParseError(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                Console.Error.WriteLine("Error when parsing options: " + error);
            }
        }
    }
}