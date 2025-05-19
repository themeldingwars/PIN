namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class OrnamentRecord
{
    // public Matrix4x4 RelativeTransform { get; set; }
    // public Matrix4x4 Pfx1Transform { get; set; }
    // public Matrix4x4 Pfx2Transform { get; set; }
    public uint Pfx1AssetId { get; set; }
    public uint Pfx2AssetId { get; set; }
    public uint AnimationNetworkId { get; set; }
    public string BoneOrHardpointName { get; set; }
    public uint VisualRecordId { get; set; }
    public uint WarpaintPaletteInfo { get; set; }
    public uint Id { get; set; }
    public byte HideInUi { get; set; }
    public byte NameIsBoneName { get; set; }
    public byte InheritedPaletteType { get; set; }
}