namespace Shared.Collision.Utilities;

public static class FileUtils
{
    public static async Task CopyAsync(string source, string destination, CancellationToken ct)
    {
        using var src = File.OpenRead(source);
        using var dest = File.Create(destination);
        await src.CopyToAsync(dest, ct);
    }
}
