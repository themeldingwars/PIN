using System;
using System.Collections.Generic;
using Autofac;
using CommandLine;
using CommandLine.Text;

namespace GameServer;

internal static class Program
{
    public static void Main(string[] arguments)
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