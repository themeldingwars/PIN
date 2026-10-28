using System;
using System.IO;

namespace Shared.Content.Tests;

/// <summary>The content in this repository has to load and validate, CI catches a broken edit before the server does.</summary>
[TestClass]
public sealed class RepositoryContentTests
{
    [TestMethod]
    public void EveryZone_LoadsWithoutProblems()
    {
        var root = ContentRoot();

        foreach (var zoneId in ContentFolder.Zones(root))
        {
            var problems = ContentValidator.Validate(ContentFolder.LoadZone(root, zoneId));

            Assert.IsEmpty(problems, string.Join("\n", problems));
        }
    }

    private static string ContentRoot()
    {
        for (var folder = new DirectoryInfo(AppContext.BaseDirectory); folder != null; folder = folder.Parent)
        {
            if (File.Exists(Path.Combine(folder.FullName, "PIN.sln")))
            {
                return Path.Combine(folder.FullName, "UdpHosts", "GameServer", "Content");
            }
        }

        throw new InvalidOperationException("Can't find PIN.sln above " + AppContext.BaseDirectory);
    }
}
