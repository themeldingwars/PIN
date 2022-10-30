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

            LoadSettings();
            ParseAndApplyCliOptions(arguments);

            Logger = new LoggerConfiguration()
                     .MinimumLevel.Is(GameServerSettings.LogLevel.Value!)
                     .WriteTo.Console(theme: SerilogTheme.Custom)
                     .CreateLogger();


            // TODO: add DI?

            PacketServer.Logger = Logger;

            
            Span<byte> ranSpan = stackalloc byte[8];
            new Random().NextBytes(ranSpan.Slice(2, 6));
            var serverId = BinaryPrimitives.ReadUInt64LittleEndian(ranSpan);
            var server = new GameServer(25001, serverId);
            server.Run();
        }

        /// <summary>
        /// Load the settings from the current directory.
        /// "settings.development.json" takes precedence over "settings.json"
        /// <remarks>The ConfigurationExtensions used in the WebHostManager are only available for the Windows platform, hence we need to implement it manually</remarks>
        /// </summary>
        private static void LoadSettings()
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "config");
            var developmentSettingsPath = Path.Combine(basePath, "settings.development.json");
            var settingsPath = Path.Combine(basePath, "settings.json");

            var sourcePath = File.Exists(developmentSettingsPath) ? developmentSettingsPath : settingsPath;

            try
            {
                var json = File.ReadAllText(sourcePath);
                GameServerSettings = JsonConvert.DeserializeObject<GameServerSettings>(json);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error when trying to read settings from \"{sourcePath}\" : {e}{Environment.NewLine}Using builtin default settings");
                GameServerSettings = new GameServerSettings { LogLevel = LogEventLevel.Verbose };
            }

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