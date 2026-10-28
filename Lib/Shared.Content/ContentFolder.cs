using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Shared.Content.Zones;

namespace Shared.Content;

/// <summary>The content folder: <c>zones/&lt;zone id&gt;/</c> with one file per kind, and <c>schemas/</c>.</summary>
public static class ContentFolder
{
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

    /// <exception cref="ContentException">A file isn't valid JSON or doesn't match its kind.</exception>
    public static ZoneContent LoadZone(string root, uint zoneId)
    {
        var folder = ZonePath(root, zoneId);
        return new ZoneContent(zoneId)
        {
            Deployables = Load(folder, ContentKinds.Deployables),
            Outposts = Load(folder, ContentKinds.Outposts),
            Meldings = Load(folder, ContentKinds.Meldings),
            Encounters = Load(folder, ContentKinds.Encounters),
        };
    }

    /// <summary>Writes every kind of the zone, a kind without entries has its file removed.</summary>
    public static void WriteZone(string root, ZoneContent zone)
    {
        var folder = ZonePath(root, zone.ZoneId);
        Write(folder, ContentKinds.Deployables, zone.Deployables);
        Write(folder, ContentKinds.Outposts, zone.Outposts);
        Write(folder, ContentKinds.Meldings, zone.Meldings);
        Write(folder, ContentKinds.Encounters, zone.Encounters);
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
