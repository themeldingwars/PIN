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
        var options = ParseCliOptions(arguments);

        using var container = CreateContainer(CommandLineOverrides(options));

        var server = container.Resolve<GameServer>();
        server.Run();
    }

    /// <summary>
    ///     Create Autofac container for dependency injection
    /// </summary>
    /// <param name="commandLineOverrides">Settings from the CLI that override the config file and the environment</param>
    private static IContainer CreateContainer(IReadOnlyDictionary<string, string> commandLineOverrides)
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterModule(new GameServerModule(commandLineOverrides));
        return containerBuilder.Build();
    }

    /// <summary>
    ///     Parse the options passed via the command line
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
    ///     Turn the parsed options into configuration values. They are the last provider in the chain,
    ///     so they win over appsettings.json and the environment. Options that weren't given stay out,
    ///     otherwise their defaults would overwrite the configured values.
    /// </summary>
    /// <param name="options">CLI Options, null when the arguments could not be parsed</param>
    private static IReadOnlyDictionary<string, string> CommandLineOverrides(CliOptions options)
    {
        var overrides = new Dictionary<string, string>();

        if (options == null)
        {
            return overrides;
        }

        if (options.LogLevel != null)
        {
            overrides["GameServer:LogLevel"] = options.LogLevel.ToString();
        }

        if (options.ForceReload)
        {
            overrides["GameServer:ForceReloadZone"] = "true";
        }

        return overrides;
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