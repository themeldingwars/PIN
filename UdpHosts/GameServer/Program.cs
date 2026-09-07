using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using CommandLine;
using CommandLine.Text;

namespace GameServer;

internal static class Program
{
    public static int Main(string[] arguments)
    {
        try
        {
            using var container = CreateContainer();

            var settings = container.Resolve<GameServerSettings>();

            var options = ParseCliOptions(arguments);
            if (options is not null)
            {
                ApplyCliOptions(options, settings);
            }

            var server = container.Resolve<GameServer>();
            server.Run();

            return 0;
        }
        catch (Exception ex)
        {
            ReportFatalError(ex);
            return 1;
        }
    }

    /// <summary>
    ///     Print a readable error for a failed startup (e.g. a missing StaticDBPath configuration)
    ///     instead of an unhandled exception with a dependency injection stack trace.
    /// </summary>
    /// <param name="ex">The exception that terminated the server</param>
    private static void ReportFatalError(Exception ex)
    {
        var reason = ex.GetBaseException();

        Console.Error.WriteLine($"GameServer terminated: {reason.Message}");
        Console.Error.WriteLine();

        if (IsMissingRuntimeAssembly(reason))
        {
            Console.Error.WriteLine("The GameServer installation is incomplete: a required .NET assembly could not be loaded.");
            Console.Error.WriteLine("Re-extract the complete PIN release archive into a fresh folder and start GameServer.exe from that folder.");
            Console.Error.WriteLine("If the error persists, download the latest PIN release: a GameServer.exe shipped next to a GameServer.dll is an outdated build.");
            return;
        }

        Console.Error.WriteLine("Check GameServer.config.json next to GameServer.exe; the README section \"GameServer config\" describes the required values.");
    }

    /// <summary>
    ///     Determine whether startup failed because a managed assembly is absent rather than because
    ///     a Firefall data path is invalid.
    /// </summary>
    /// <param name="exception">The root startup exception.</param>
    /// <returns><c>true</c> when the exception reports a missing managed assembly.</returns>
    private static bool IsMissingRuntimeAssembly(Exception exception)
    {
        return (exception is FileNotFoundException or FileLoadException) &&
               exception.Message.Contains("Could not load file or assembly", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Create Autofac container for dependency injection
    /// </summary>
    private static IContainer CreateContainer()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterModule<GameServerModule>();
        return containerBuilder.Build();
    }

    /// <summary>
    ///     Parse the options passed via the command line and overwrite settings from the config
    /// </summary>
    /// <param name="arguments">CLI Arguments</param>
    private static CliOptions ParseCliOptions(IEnumerable<string> arguments)
    {
        var parser = new Parser();
        var parserResult = parser.ParseArguments<CliOptions>(arguments);
        CliOptions options = null;
        parserResult.WithParsed(o => options = o)
                    .WithNotParsed(_ => DisplayHelpText(parserResult));

        return options;
    }

    /// <summary>
    ///     Handle the parsed options, essentially overwriting already present settings loaded from App.config
    /// </summary>
    /// <param name="options">CLI Options</param>
    /// <param name="settings">Game Server Settings</param>
    private static void ApplyCliOptions(CliOptions options, GameServerSettings settings)
    {
        if (options.LogLevel != null)
        {
            settings.LogLevel = options.LogLevel;
        }

        if (options.ForceReload)
        {
            settings.ForceReloadZone = options.ForceReload;
        }
    }

    /// <summary>
    ///     If errors occur during the parsing of CLI options, they should be handled here
    /// </summary>
    /// <param name="result">Parser result</param>
    private static void DisplayHelpText<T>(ParserResult<T> result)
    {
        var helpText = HelpText.AutoBuild(result,
                                          h =>
                                                  {
                                                      h.AdditionalNewLineAfterOption = false;
                                                      return HelpText.DefaultParsingErrorsHandler(result, h);
                                                  }, 
                                          e => e);
        Console.WriteLine(helpText);
    }
}