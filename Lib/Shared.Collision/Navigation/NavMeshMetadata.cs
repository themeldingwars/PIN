using System.IO;
using System.Text.Json;

namespace Shared.Collision.Navigation;

/// <summary>
///     Sidecar written next to a baked navmesh recording the settings it was baked with. The game
///     server compares <see cref="ParamsHash" /> against the current <see cref="NavMeshBuildParams" />
///     and refuses to load a navmesh that no longer matches, so changing a bake parameter can never
///     silently leave a stale <c>.nav</c> in the cache.
/// </summary>
public sealed record NavMeshMetadata
{
    /// <summary>File name suffix appended to the zone id, e.g. <c>1003.nav.meta</c>.</summary>
    public const string Suffix = ".nav.meta";

    /// <summary>Bumped when the layout of the <c>.nav</c> itself changes, so older files are rejected.</summary>
    public const int CurrentFormatVersion = 1;

    private static readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    public int FormatVersion { get; init; } = CurrentFormatVersion;

    /// <summary>The <see cref="NavMeshBuildParams.ComputeHash" /> the mesh was baked with.</summary>
    public long ParamsHash { get; init; }

    public uint ZoneId { get; init; }

    public int TileCount { get; init; }

    public int PolyCount { get; init; }

    public int DetailTriCount { get; init; }

    public DateTime BuiltUtc { get; init; }

    public static string PathFor(string navmeshDirectory, uint zoneId)
    {
        return Path.Combine(navmeshDirectory, $"{zoneId}{Suffix}");
    }

    public void Save(string navmeshDirectory, uint zoneId)
    {
        File.WriteAllText(PathFor(navmeshDirectory, zoneId), JsonSerializer.Serialize(this, _serializerOptions));
    }

    /// <summary>Reads the sidecar, or null when it is missing or unreadable (an untracked bake).</summary>
    public static NavMeshMetadata? Load(string navmeshDirectory, uint zoneId)
    {
        var path = PathFor(navmeshDirectory, zoneId);
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<NavMeshMetadata>(File.ReadAllText(path));
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            return null;
        }
    }

    /// <summary>True when this sidecar describes a mesh baked with exactly these parameters.</summary>
    public bool IsCurrentFor(NavMeshBuildParams parameters)
    {
        return FormatVersion == CurrentFormatVersion && ParamsHash == parameters.ComputeHash();
    }
}
