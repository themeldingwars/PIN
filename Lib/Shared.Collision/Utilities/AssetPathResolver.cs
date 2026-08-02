#nullable enable
using System.Collections.Concurrent;

namespace Shared.Collision.Utilities;

public static class AssetPathResolver
{
    public static string ComputeFolderName(string assetId)
    {
        return (int.Parse(assetId) / 1000 * 1000).ToString("D8");
    }

    public static string[] ScanAvailableFolders(string assetRoot)
    {
        return [.. Directory.GetDirectories(assetRoot)
            .Select(Path.GetFileName)
            .Where(name => name != null && name.All(char.IsDigit))
            .Select(name => name!)];
    }

    public static string? Resolve(
        string assetRoot,
        string assetId,
        string extension,
        HashSet<string> availableFolders,
        ConcurrentDictionary<string, string>? cache = null)
    {
        var cacheKey = $"{assetId}{extension}";

        if (cache != null && cache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        string folderName = ComputeFolderName(assetId);

        if (!availableFolders.Contains(folderName))
        {
            return null;
        }

        string folderPath = Path.Combine(assetRoot, folderName);
        if (!Directory.Exists(folderPath))
        {
            return null;
        }

        var file = Directory.EnumerateFiles(folderPath, $"*{extension}")
            .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == assetId);

        if (file != null)
        {
            cache?.TryAdd(cacheKey, file);
            return file;
        }

        return null;
    }
}
