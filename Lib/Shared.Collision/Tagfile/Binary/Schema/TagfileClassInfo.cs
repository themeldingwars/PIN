namespace Shared.Collision.Tagfile.Binary.Schema;

public class TagfileClassInfo
{
    public string Name { get; set; } = string.Empty;
    public string? ParentName { get; set; }
    public int ObjectSize { get; set; }
    public List<TagfileMemberInfo> Members { get; set; } = [];
    public List<TagfileEnumInfo> Enums { get; set; } = [];
}
