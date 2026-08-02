using System.Runtime.InteropServices;
using Serilog;
using Shared.Collision.Tagfile.Models;

namespace Shared.Collision.Tagfile;

[StructLayout(LayoutKind.Sequential)]
public class TagfileAsset : ITagfileExternalStorage
{
    private static readonly ILogger _logger = Log.ForContext<TagfileAsset>();

    public VertBlockContent[] VertBlocks { get; set; } = [];
    public IndiceBlockContent[] IndiceBlocks { get; set; } = [];
    public Dictionary<string, BaseTagfileObject> TagfileObjects { get; set; } = [];

    public BaseTagfileObject? GetTagfileObject(string query)
    {
        TagfileObjects.TryGetValue(query, out BaseTagfileObject? result);

        if (result != null)
        {
            return result;
        }

        _logger.Error("Failed to find TagfileDictObject with query {query}", query);
        return null;
    }
}
