using System;
using System.IO;
using Xunit;

namespace GameServer.Tests;

/// <summary>
/// Tests for <see cref="AppConfigFile" />, the replacement for
/// <c>ConfigurationManager.AppSettings</c>. ConfigurationManager resolves its file through
/// <c>Assembly.CodeBase</c>, which throws "CodeBase is not supported on assemblies loaded from a
/// single-file bundle" in the published GameServer.exe, so the XML is parsed from disk instead.
/// </summary>
public sealed class AppConfigFileTests : IDisposable
{
    private readonly string directory;

    public AppConfigFileTests()
    {
        directory = Path.Combine(Path.GetTempPath(), "pin-appconfig-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(directory, true);
        }
        catch (IOException)
        {
            // A leftover temp directory must not fail the test run.
        }
    }

    [Fact]
    public void ParseAppSettings_ReadsKeysAndValues()
    {
        var settings = AppConfigFile.ParseAppSettings(
            """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
                <appSettings>
                    <add key="Port" value="25001"/>
                    <add key="StaticDBPath" value="C:\Firefall\system\db\clientdb.sd2"/>
                    <add key="serilog:minimum-level" value="Debug"/>
                </appSettings>
            </configuration>
            """,
            "test");

        Assert.Equal("25001", settings["Port"]);
        Assert.Equal(@"C:\Firefall\system\db\clientdb.sd2", settings["StaticDBPath"]);
        Assert.Equal("Debug", settings["serilog:minimum-level"]);
        Assert.Null(settings["Missing"]);
    }

    [Fact]
    public void ParseAppSettings_LooksKeysUpCaseInsensitivelyLikeConfigurationManager()
    {
        var settings = AppConfigFile.ParseAppSettings(
            "<configuration><appSettings><add key=\"ZoneId\" value=\"448\"/></appSettings></configuration>",
            "test");

        Assert.Equal("448", settings["zoneid"]);
        Assert.Equal("448", settings["ZONEID"]);
    }

    [Fact]
    public void ParseAppSettings_KeepsTheLastValueOfARepeatedKey()
    {
        var settings = AppConfigFile.ParseAppSettings(
            "<configuration><appSettings><add key=\"Port\" value=\"1\"/><add key=\"Port\" value=\"2\"/></appSettings></configuration>",
            "test");

        // NameValueCollection.Add() would join these into "1,2" and break ushort.Parse.
        Assert.Equal("2", settings["Port"]);
    }

    [Fact]
    public void ParseAppSettings_HonoursRemoveAndClear()
    {
        var settings = AppConfigFile.ParseAppSettings(
            """
            <configuration>
                <appSettings>
                    <add key="Dropped" value="1"/>
                    <add key="Wiped" value="1"/>
                    <remove key="Dropped"/>
                    <clear/>
                    <add key="Kept" value="1"/>
                </appSettings>
            </configuration>
            """,
            "test");

        Assert.Null(settings["Dropped"]);
        Assert.Null(settings["Wiped"]);
        Assert.Equal("1", settings["Kept"]);
    }

    [Fact]
    public void ParseAppSettings_TreatsAMissingValueAsEmptyAndIgnoresOtherSections()
    {
        var settings = AppConfigFile.ParseAppSettings(
            """
            <configuration>
                <startup><supportedRuntime version="v4.0"/></startup>
                <appSettings>
                    <add key="Empty"/>
                    <!-- comment -->
                </appSettings>
            </configuration>
            """,
            "test");

        Assert.Equal(string.Empty, settings["Empty"]);
        Assert.Null(settings["supportedRuntime"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("<configuration></configuration>")]
    [InlineData("<configuration><connectionStrings/></configuration>")]
    public void ParseAppSettings_ReturnsEmptySettingsWhenThereIsNoAppSettingsSection(string xml)
    {
        var settings = AppConfigFile.ParseAppSettings(xml, "test");

        Assert.Equal(0, settings.Count);
    }

    [Fact]
    public void ParseAppSettings_ReportsTheFileWhenTheXmlIsBroken()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => AppConfigFile.ParseAppSettings("<configuration><appSettings>", "GameServer.dll.config"));

        Assert.Contains("GameServer.dll.config", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadAppSettings_ReadsAConfigFileFromDisk()
    {
        var path = Path.Combine(directory, "GameServer.dll.config");
        File.WriteAllText(path, "<configuration><appSettings><add key=\"Port\" value=\"25099\"/></appSettings></configuration>");

        Assert.Equal("25099", AppConfigFile.ReadAppSettings(path)["Port"]);
    }

    [Fact]
    public void CandidateFileNames_PrefersTheSdkProducedNameAndIncludesTheApphostAndProjectNames()
    {
        var names = AppConfigFile.CandidateFileNames("GameServer", "GameServer.exe");

        Assert.Equal("GameServer.dll.config", names[0]);
        Assert.Contains("GameServer.exe.config", names);
        Assert.Contains("App.config", names);
    }

    [Fact]
    public void CandidateFileNames_IgnoresTheSharedDotnetHost()
    {
        var names = AppConfigFile.CandidateFileNames("GameServer", "dotnet.exe");

        Assert.DoesNotContain("dotnet.exe.config", names);
        Assert.DoesNotContain("dotnet.dll.config", names);
        Assert.Contains("GameServer.dll.config", names);
    }

    [Fact]
    public void CandidateFileNames_StillWorksWithoutAnEntryAssemblyName()
    {
        var names = AppConfigFile.CandidateFileNames(null, "GameServer.exe");

        Assert.Contains("GameServer.exe.config", names);
        Assert.Contains("App.config", names);
    }

    [Fact]
    public void FindConfigFile_ReturnsTheFirstMatchInFileNameOrder()
    {
        File.WriteAllText(Path.Combine(directory, "App.config"), "<configuration/>");
        File.WriteAllText(Path.Combine(directory, "GameServer.dll.config"), "<configuration/>");

        var found = AppConfigFile.FindConfigFile(
            new[] { directory },
            new[] { "GameServer.dll.config", "App.config" });

        Assert.Equal(Path.Combine(directory, "GameServer.dll.config"), found);
    }

    [Fact]
    public void FindConfigFile_SearchesLaterDirectoriesAndSkipsEmptyOnes()
    {
        var second = Path.Combine(directory, "second");
        Directory.CreateDirectory(second);
        File.WriteAllText(Path.Combine(second, "App.config"), "<configuration/>");

        var found = AppConfigFile.FindConfigFile(
            new[] { null, string.Empty, directory, second },
            new[] { "App.config" });

        Assert.Equal(Path.Combine(second, "App.config"), found);
    }

    [Fact]
    public void FindConfigFile_ReturnsNullWhenNothingExists()
    {
        Assert.Null(AppConfigFile.FindConfigFile(new[] { directory }, new[] { "App.config" }));
    }

    [Fact]
    public void Settings_AreNeverNullSoStartupFallsBackToDefaults()
    {
        Assert.NotNull(AppConfigFile.Settings);
    }
}
