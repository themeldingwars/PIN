using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace GameServer;

/// <summary>
///     Reads the legacy XML <c>appSettings</c> block (<c>App.config</c> in the source tree,
///     <c>GameServer.dll.config</c> next to the built or published executable) directly from disk.
/// </summary>
/// <remarks>
///     <para>
///         <c>System.Configuration.ConfigurationManager</c> resolves the configuration file location through
///         <c>Assembly.CodeBase</c>. That property is not supported inside a single-file bundle, so in the
///         published <c>GameServer.exe</c> the very first settings lookup threw
///         <c>NotSupportedException: CodeBase is not supported on assemblies loaded from a single-file bundle</c>.
///         That happened before <c>GameServer.config.json</c> was ever read, which is why setting the Firefall
///         paths by hand did not help.
///     </para>
///     <para>
///         Parsing the file here keeps the same configuration format working for local builds and for the
///         single-file release build, and keeps <c>Assembly.CodeBase</c> out of the startup path entirely.
///     </para>
/// </remarks>
public static class AppConfigFile
{
    private const string AppSettingsElement = "appSettings";
    private const string KeyAttribute = "key";
    private const string ValueAttribute = "value";

    private static readonly Lazy<LoadedAppConfig> Loaded = new(Load);

    /// <summary>
    ///     Full path of the configuration file the settings were read from,
    ///     or <c>null</c> when no configuration file exists (defaults are then used).
    /// </summary>
    public static string FilePath => Loaded.Value.FilePath;

    /// <summary>
    ///     The <c>appSettings</c> entries, keyed case-insensitively just like
    ///     <c>ConfigurationManager.AppSettings</c>. Never <c>null</c>; empty when no file was found.
    /// </summary>
    public static NameValueCollection Settings => Loaded.Value.Settings;

    /// <summary>
    ///     Build the ordered list of configuration file names to look for.
    /// </summary>
    /// <param name="entryAssemblyName">Simple name of the entry assembly, e.g. <c>GameServer</c>.</param>
    /// <param name="processFileName">File name of the running process, e.g. <c>GameServer.exe</c>.</param>
    /// <returns>Candidate file names, most specific first.</returns>
    public static IReadOnlyList<string> CandidateFileNames(string entryAssemblyName, string processFileName)
    {
        var names = new List<string>();

        void AddName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name) && !names.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                names.Add(name);
            }
        }

        // What the SDK actually produces for an App.config: <AssemblyName>.dll.config.
        if (!string.IsNullOrWhiteSpace(entryAssemblyName))
        {
            AddName($"{entryAssemblyName}.dll.config");
            AddName($"{entryAssemblyName}.exe.config");
        }

        // The apphost may be named differently from the assembly. Ignore the shared "dotnet" host:
        // dotnet.exe.config belongs to the runtime installation, not to this server.
        if (!string.IsNullOrWhiteSpace(processFileName) && !IsSharedHost(processFileName))
        {
            AddName($"{processFileName}.config");
            AddName($"{Path.GetFileNameWithoutExtension(processFileName)}.dll.config");
        }

        // Running straight out of a project directory.
        AddName("App.config");

        return names;
    }

    /// <summary>
    ///     Return the first existing candidate file, searching each directory in order.
    /// </summary>
    /// <param name="directories">Directories to search, most authoritative first.</param>
    /// <param name="fileNames">Candidate file names, most specific first.</param>
    /// <returns>The full path of the first file found, or <c>null</c>.</returns>
    public static string FindConfigFile(IEnumerable<string> directories, IEnumerable<string> fileNames)
    {
        var names = fileNames as IReadOnlyList<string> ?? fileNames.ToList();

        foreach (var directory in directories.Where(candidate => !string.IsNullOrWhiteSpace(candidate)))
        {
            foreach (var name in names)
            {
                string path;

                try
                {
                    path = Path.Combine(directory, name);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                if (File.Exists(path))
                {
                    return path;
                }
            }
        }

        return null;
    }

    /// <summary>
    ///     Read the <c>appSettings</c> entries of a configuration file.
    /// </summary>
    /// <param name="path">Full path of the configuration file.</param>
    /// <returns>The settings it declares; empty when it has no <c>appSettings</c> section.</returns>
    public static NameValueCollection ReadAppSettings(string path)
    {
        string xml;

        try
        {
            xml = File.ReadAllText(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                $"Could not read configuration file '{path}'. Fix or remove it and try again.",
                ex);
        }

        return ParseAppSettings(xml, path);
    }

    /// <summary>
    ///     Parse the <c>appSettings</c> entries out of a configuration document, honouring
    ///     <c>add</c>, <c>remove</c> and <c>clear</c> in document order like the .NET configuration system does.
    /// </summary>
    /// <param name="xml">Contents of the configuration file.</param>
    /// <param name="source">Path or description used in error messages.</param>
    /// <returns>The settings the document declares.</returns>
    public static NameValueCollection ParseAppSettings(string xml, string source)
    {
        var settings = CreateSettings();

        if (string.IsNullOrWhiteSpace(xml))
        {
            return settings;
        }

        XDocument document;

        try
        {
            document = XDocument.Parse(xml);
        }
        catch (XmlException ex)
        {
            throw new InvalidOperationException(
                $"Configuration file '{source}' is not valid XML: {ex.Message} Fix or remove it and try again.",
                ex);
        }

        var appSettings = document.Root?.Elements()
                                  .FirstOrDefault(element => element.Name.LocalName == AppSettingsElement);
        if (appSettings == null)
        {
            return settings;
        }

        foreach (var element in appSettings.Elements())
        {
            switch (element.Name.LocalName)
            {
                case "add":
                    var key = element.Attribute(KeyAttribute)?.Value;
                    if (!string.IsNullOrEmpty(key))
                    {
                        // Set() replaces: a repeated key keeps the last value instead of joining them.
                        settings.Set(key, element.Attribute(ValueAttribute)?.Value ?? string.Empty);
                    }

                    break;

                case "remove":
                    var removedKey = element.Attribute(KeyAttribute)?.Value;
                    if (!string.IsNullOrEmpty(removedKey))
                    {
                        settings.Remove(removedKey);
                    }

                    break;

                case "clear":
                    settings.Clear();
                    break;

                default:
                    break;
            }
        }

        return settings;
    }

    /// <summary>
    ///     Create an empty, case-insensitive settings collection matching <c>ConfigurationManager.AppSettings</c>.
    /// </summary>
    /// <returns>An empty settings collection.</returns>
    private static NameValueCollection CreateSettings()
    {
        return new NameValueCollection(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Locate and read the configuration file once, on first use.
    /// </summary>
    /// <returns>The loaded configuration file, or an empty one when none exists.</returns>
    private static LoadedAppConfig Load()
    {
        var directories = new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() };
        var fileNames = CandidateFileNames(Assembly.GetEntryAssembly()?.GetName().Name, GetProcessFileName());

        var path = FindConfigFile(directories, fileNames);

        return path == null
                   ? new LoadedAppConfig(null, CreateSettings())
                   : new LoadedAppConfig(path, ReadAppSettings(path));
    }

    /// <summary>
    ///     File name of the running executable, or <c>null</c> when the host does not expose it.
    /// </summary>
    /// <returns>The process file name, e.g. <c>GameServer.exe</c>.</returns>
    private static string GetProcessFileName()
    {
        var processPath = Environment.ProcessPath;

        return string.IsNullOrWhiteSpace(processPath) ? null : Path.GetFileName(processPath);
    }

    /// <summary>
    ///     Determine whether the process is the shared <c>dotnet</c> host rather than an apphost of this server.
    /// </summary>
    /// <param name="processFileName">File name of the running process.</param>
    /// <returns><c>true</c> when the app runs through the shared host.</returns>
    private static bool IsSharedHost(string processFileName)
    {
        return string.Equals(processFileName, "dotnet", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(processFileName, "dotnet.exe", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     The configuration file that was loaded at startup.
    /// </summary>
    /// <param name="FilePath">Path it was read from, or <c>null</c> when no file exists.</param>
    /// <param name="Settings">The <c>appSettings</c> entries it declares.</param>
    private sealed record LoadedAppConfig(string FilePath, NameValueCollection Settings);
}
