using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Shared.Content.Spawns;
using Shared.Content.Zones;

namespace Shared.Content;

/// <summary>The content folder: <c>zones/&lt;zone id&gt;/</c> with one file per kind, <c>spawn-tables/</c> and <c>schemas/</c>.</summary>
public static class ContentFolder
{
    private const string _zoneSettingsSchema = "../../schemas/zone.schema.json";
    private const string _zoneSettingsFileName = "zone.json";

    public static string ZonePath(string root, uint zoneId) => Path.Combine(root, "zones", zoneId.ToString(CultureInfo.InvariantCulture));

    public static IReadOnlyList<uint> Zones(string root)
    {
        var zones = Path.Combine(root, "zones");
        if (!Directory.Exists(zones))
        {
            return [];
        }

        return [.. Directory.EnumerateDirectories(zones)
            .Select(Path.GetFileName)
            .Select(name => uint.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out var id) ? id : (uint?)null)
            .OfType<uint>()
            .Order()];
    }

    /// <summary>The zone a file under <c>zones/</c> belongs to, false for anything else.</summary>
    public static bool TryGetZone(string root, string path, out uint zoneId)
    {
        zoneId = 0;
        var relative = Path.GetRelativePath(Path.Combine(root, "zones"), path);
        var parts = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return parts.Length == 2 && uint.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out zoneId);
    }

    public static string SpawnTablesPath(string root) => Path.Combine(root, ContentKinds.SpawnTables.Name);

    /// <summary>Whether the file is one of the spawn table files, whose folder has no subfolders.</summary>
    public static bool IsSpawnTableFile(string root, string path)
    {
        var relative = Path.GetRelativePath(SpawnTablesPath(root), path);
        return !relative.StartsWith("..", System.StringComparison.Ordinal) && !Path.IsPathRooted(relative)
            && relative.IndexOfAny([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]) < 0
            && Path.GetExtension(relative) == ".json";
    }

    /// <exception cref="ContentException">A file isn't valid JSON or doesn't match its kind.</exception>
    public static IReadOnlyList<SpawnTableFile> LoadSpawnTables(string root)
    {
        var folder = SpawnTablesPath(root);
        if (!Directory.Exists(folder))
        {
            return [];
        }

        return [.. Directory.EnumerateFiles(folder, "*.json")
            .Order(System.StringComparer.Ordinal)
            .Select(path => new SpawnTableFile(Path.GetFileNameWithoutExtension(path), ContentJson.ReadFile(ContentKinds.SpawnTables, path)))];
    }

    public static void WriteSpawnTables(string root, SpawnTableFile file)
    {
        var folder = SpawnTablesPath(root);
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, $"{file.Family}.json"), ContentJson.Write(ContentKinds.SpawnTables, file.Tables));
    }

    /// <exception cref="ContentException">A file isn't valid JSON or doesn't match its kind.</exception>
    public static ZoneContent LoadZone(string root, uint zoneId)
    {
        var folder = ZonePath(root, zoneId);
        var settings = Path.Combine(folder, _zoneSettingsFileName);
        return new ZoneContent(zoneId)
        {
            Settings = File.Exists(settings) ? ContentJson.ReadObject<ZoneSettings>(File.ReadAllText(settings), settings) : new ZoneSettings(),
            Deployables = Load(folder, ContentKinds.Deployables),
            Outposts = Load(folder, ContentKinds.Outposts),
            Meldings = Load(folder, ContentKinds.Meldings),
            Encounters = Load(folder, ContentKinds.Encounters),
            Regions = Load(folder, ContentKinds.Regions),
        };
    }

    /// <summary>
    ///     Writes every kind of the zone, a kind without entries has its file removed, and so has <c>zone.json</c> when
    ///     the settings are the defaults.
    /// </summary>
    public static void WriteZone(string root, ZoneContent zone)
    {
        var folder = ZonePath(root, zone.ZoneId);
        var settings = Path.Combine(folder, _zoneSettingsFileName);
        if (zone.Settings == new ZoneSettings())
        {
            File.Delete(settings);
        }
        else
        {
            Directory.CreateDirectory(folder);
            File.WriteAllText(settings, ContentJson.WriteObject(zone.Settings, _zoneSettingsSchema));
        }

        Write(folder, ContentKinds.Deployables, zone.Deployables);
        Write(folder, ContentKinds.Outposts, zone.Outposts);
        Write(folder, ContentKinds.Meldings, zone.Meldings);
        Write(folder, ContentKinds.Encounters, zone.Encounters);
        Write(folder, ContentKinds.Regions, zone.Regions);
    }

    private static IReadOnlyList<T> Load<T>(string folder, ContentKind<T> kind)
        where T : ContentEntry
    {
        var path = Path.Combine(folder, kind.FileName);
        return File.Exists(path) ? ContentJson.ReadFile(kind, path) : [];
    }

    private static void Write<T>(string folder, ContentKind<T> kind, IReadOnlyList<T> entries)
        where T : ContentEntry
    {
        var path = Path.Combine(folder, kind.FileName);
        if (entries.Count == 0)
        {
            File.Delete(path);
            return;
        }

        Directory.CreateDirectory(folder);
        File.WriteAllText(path, ContentJson.Write(kind, entries));
    }
}
