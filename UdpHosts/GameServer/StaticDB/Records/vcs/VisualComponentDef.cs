namespace GameServer.Data.SDB.Records.vcs;
public record class VisualComponentDef
{
    // public Vec3 Offset { get; set; }
    public uint VisualrecId { get; set; }
    public uint AnimnetId { get; set; }
    public string Hardpoint { get; set; }
    public uint Id { get; set; }
    public byte Rotate180 { get; set; }
}
