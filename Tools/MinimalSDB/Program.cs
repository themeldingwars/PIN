using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using FauFau.Formats;

var cfgPath = new[] { Environment.CurrentDirectory, AppContext.BaseDirectory }
    .Select(dir => Path.Combine(dir, "config.json"))
    .FirstOrDefault(File.Exists);

if (cfgPath == null)
{
    Console.Error.WriteLine("Error: config.json not found in current or base directory.");
    return 1;
}

var json = JsonDocument.Parse(File.ReadAllText(cfgPath)).RootElement;
string input = json.TryGetProperty("input", out var inp) ? inp.GetString() : null;
string output = json.TryGetProperty("output", out var outP) ? outP.GetString() : null;

if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(output))
{
    Console.Error.WriteLine("Error: Invalid config.json. Both 'input' and 'output' paths are required.");
    return 2;
}

var loaderPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\..\UdpHosts\GameServer\StaticDB\Loaders\StaticDBLoader.cs"));
if (!File.Exists(loaderPath))
{
    Console.Error.WriteLine("Error: Missing loader file.");
    return 3;
}

if (!File.Exists(input))
{
    Console.Error.WriteLine("Error: Missing input file.");
    return 4;
}

Console.WriteLine($"MinimalSDB: Reading SDB: {input}");
var sdb = new StaticDB();
sdb.Read(input);

var requiredTableIds = new HashSet<uint>();
var tableRegex = TableNameRegex();

foreach (var line in File.ReadLines(loaderPath))
{
    var match = tableRegex.Match(line);
    if (!match.Success)
    {
        continue;
    }

    var tableName = match.Groups[1].Value;
    int idx = sdb.GetIndexByName(tableName);

    if (idx == -1)
    {
        Console.Error.WriteLine($"Unknown table in loader: {tableName}\n  {line.Trim()}");
        return 5;
    }

    requiredTableIds.Add(sdb.Tables[idx].Id);
}

int initialCount = sdb.Tables.Count;
sdb.Tables.RemoveAll(t => !requiredTableIds.Contains(t.Id));
Console.WriteLine($"Pruned {initialCount - sdb.Tables.Count} unused tables. Keeping {sdb.Tables.Count}.");

Console.WriteLine($"MinimalSDB: Writing SDB: {output}");
sdb.Write(output);
return 0;

partial class Program
{
    [GeneratedRegex(@">\(""([^""]+)""\)")]
    private static partial Regex TableNameRegex();
}
