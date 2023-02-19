using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Shared.Common;

public static class LocationProvider
{
    public static string FindTheMeldingWarsLocation()
    {
        var localAppData = Environment.ExpandEnvironmentVariables(@"%LocalAppData%");

        return string.IsNullOrEmpty(localAppData) ? null : Path.Combine(Environment.ExpandEnvironmentVariables(@"%LocalAppData%"), "TheMeldingWars");
    }

    public static string FindFirefallLocation()
    {
        return FindFirefallByShellCommand() ?? FindSteamLibraryLocation();
    }

    private static string FindSteamLibraryLocation()
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        using var key = Registry.LocalMachine.OpenSubKey("Software\\WOW6432Node\\Valve\\Steam");
        var steamInstallPath = (string)key?.GetValue("InstallPath");
        if (steamInstallPath == null)
        {
            return null;
        }

        var firefallSteamManifest = Path.Combine(steamInstallPath, "steamapps", "appmanifest_227700.acf");
        var firefallLocation = Path.Combine(steamInstallPath, "steamapps", "common", "Firefall");
        if (File.Exists(firefallSteamManifest) && Directory.Exists(firefallLocation))
        {
            return firefallLocation;
        }

        return null;
    }

    private static string FindFirefallByShellCommand()
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        using var shellKey = Registry.ClassesRoot.OpenSubKey("firefall\\shell\\open\\command");
        var firefallLaunchCommand = (string)shellKey?.GetValue("");
        if (firefallLaunchCommand == null)
        {
            return null;
        }

        var matches = Regex.Match(firefallLaunchCommand, "\\/D\"(.*)\" \"");
        if (matches.Groups.Count < 2)
        {
            return null;
        }

        var firefallExecutable = matches.Groups[1].Value;
        var cutOffPosition = firefallExecutable.LastIndexOf("\\system\\", StringComparison.OrdinalIgnoreCase);
        if (cutOffPosition == -1)
        {
            return null;
        }

        var firefallLocation = firefallExecutable[..cutOffPosition];
        return firefallLocation;
    }
}