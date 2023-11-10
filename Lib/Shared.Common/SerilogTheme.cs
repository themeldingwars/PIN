using System;
using System.Collections.Generic;
using Serilog.Sinks.SystemConsole.Themes;

namespace Shared.Common;

public static class SerilogTheme
{
    public static SystemConsoleTheme Custom { get; } = new(
                                                           new Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle>
                                                           {
                                                               [ConsoleThemeStyle.Text] = new() { Foreground = ConsoleColor.White },
                                                               [ConsoleThemeStyle.SecondaryText] =
                                                                   new() { Foreground = ConsoleColor.Gray },
                                                               [ConsoleThemeStyle.TertiaryText] =
                                                                   new() { Foreground = ConsoleColor.DarkGray },
                                                               [ConsoleThemeStyle.Invalid] = new() { Foreground = ConsoleColor.Yellow },
                                                               [ConsoleThemeStyle.Null] = new() { Foreground = ConsoleColor.Blue },
                                                               [ConsoleThemeStyle.Name] = new() { Foreground = ConsoleColor.Gray },
                                                               [ConsoleThemeStyle.String] = new() { Foreground = ConsoleColor.Cyan },
                                                               [ConsoleThemeStyle.Number] = new() { Foreground = ConsoleColor.Magenta },
                                                               [ConsoleThemeStyle.Boolean] = new() { Foreground = ConsoleColor.Blue },
                                                               [ConsoleThemeStyle.Scalar] = new() { Foreground = ConsoleColor.Green },
                                                               [ConsoleThemeStyle.LevelVerbose] =
                                                                   new() { Foreground = ConsoleColor.Gray, Background = ConsoleColor.DarkGray },
                                                               [ConsoleThemeStyle.LevelDebug] =
                                                                   new() { Foreground = ConsoleColor.White, Background = ConsoleColor.DarkGray },
                                                               [ConsoleThemeStyle.LevelInformation] =
                                                                   new() { Foreground = ConsoleColor.White, Background = ConsoleColor.Blue },
                                                               [ConsoleThemeStyle.LevelWarning] =
                                                                   new() { Foreground = ConsoleColor.DarkGray, Background = ConsoleColor.Yellow },
                                                               [ConsoleThemeStyle.LevelError] =
                                                                   new() { Foreground = ConsoleColor.White, Background = ConsoleColor.Red },
                                                               [ConsoleThemeStyle.LevelFatal] =
                                                                   new() { Foreground = ConsoleColor.White, Background = ConsoleColor.Red }
                                                           });
}