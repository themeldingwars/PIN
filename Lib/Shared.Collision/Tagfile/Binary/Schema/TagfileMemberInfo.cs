namespace Shared.Collision.Tagfile.Binary.Schema;

public class TagfileMemberInfo
{
    public string ClassName { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Subtype { get; set; } =  string.Empty;
    public string? ResolvedClassName { get; set; }
    public string? ResolvedEnumName { get; set; }
    public int CArraySize { get; set; }
    public string Flags { get; set; } = string.Empty;
    public int Offset { get; set; }
}
