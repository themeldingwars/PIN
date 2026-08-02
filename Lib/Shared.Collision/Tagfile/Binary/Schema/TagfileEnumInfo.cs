namespace Shared.Collision.Tagfile.Binary.Schema;

public class TagfileEnumInfo
{
    public string Name { get; set; } = string.Empty;
    public List<TagfileEnumValueInfo> Values { get; set; } = [];
}
