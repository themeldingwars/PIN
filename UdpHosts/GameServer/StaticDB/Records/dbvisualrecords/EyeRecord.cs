namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class EyeRecord
{
    // public Vec4 ScleraColor { get; set; }
    public float EyeColorBleed { get; set; }
    public float EyeBoostExp { get; set; }
    public uint IrisTextureAssetId { get; set; }
    public float StromaTile { get; set; }
    public float IrisSize { get; set; }
    public float PupilSize { get; set; }
    public uint Id { get; set; }
}
