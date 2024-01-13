namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class TattooDecal
{
    public uint LocalizedNameId { get; set; }
    public uint DisplayFlags { get; set; }
    public float AspectRatio { get; set; }
    public uint TextureAssetId { get; set; }
    public uint Id { get; set; }
    public byte TypeFlags { get; set; }
}
