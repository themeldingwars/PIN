#nullable enable

using System;
using System.IO;
using System.Text.Json;

namespace CollisionGenerator;

public sealed record CacheManifest(
    int FormatVersion,
    string? ChunkName,
    bool AllChunks,
    uint? ZoneId,
    string? AssetId,
    bool AllAssets,
    string? MapsFingerprint,
    string? AssetDbFingerprint)
{   
    private const string _fileName = "manifest.json";
    private const int _currentFormatVersion = 1;

    private static readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    public static CacheManifest FromOptions(Options opts)
    {
        return new CacheManifest(
            _currentFormatVersion,
            opts.ChunkName,
            opts.AllChunks || opts.AllZones,
            opts.ZoneId,
            opts.AssetId,
            opts.AllAssets,
            Fingerprint(opts.MapsPath),
            Fingerprint(opts.AssetDbPath));
    }

    public static CacheManifest? Load(string cachePath)
    {
        var path = Path.Combine(cachePath, _fileName);
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CacheManifest>(File.ReadAllText(path));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static void Delete(string cachePath)
    {
        File.Delete(Path.Combine(cachePath, _fileName));
    }

    public void Save(string cachePath)
    {
        File.WriteAllText(Path.Combine(cachePath, _fileName), JsonSerializer.Serialize(this, _serializerOptions));
    }

    private static string? Fingerprint(string? directory)
    {
        if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
        {
            return null;
        }

        // Cheap fingerprint of a directory tree: file count, total size and newest write time.
        var count = 0;
        var totalSize = 0L;
        var newest = 0L;
        foreach (var file in new DirectoryInfo(directory).EnumerateFiles("*", SearchOption.AllDirectories))
        {
            count++;
            totalSize += file.Length;
            newest = Math.Max(newest, file.LastWriteTimeUtc.Ticks);
        }

        return FormattableString.Invariant($"{count}:{totalSize}:{newest}");
    }
}
