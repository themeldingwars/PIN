namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class VisualInfoGroupEntry
{
    // public Vec4 ExtraColor3 { get; set; }
    // public Vec4 ExtraColor4 { get; set; }
    // public Vec4 ExtraColor1 { get; set; }
    // public Vec4 ExtraColor2 { get; set; }
    // public Vec3 RandRotation { get; set; }
    // public Vec3 FixedOffset { get; set; }
    public uint VisualrecId { get; set; }
    public uint SubgroupIndex { get; set; }
    public float RandScaleMin { get; set; }
    public uint VisualinfoGroupId { get; set; }
    public uint CollisionOverrideId { get; set; }
    public float RandScaleMax { get; set; }
    public uint AnimnetworkId { get; set; }
}
