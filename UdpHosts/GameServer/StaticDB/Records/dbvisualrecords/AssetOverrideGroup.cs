namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class AssetOverrideGroup
{
    public uint DefaultAssetId { get; set; }
    public uint Id { get; set; }
    public uint Priority { get; set; }
    public byte AssetType { get; set; }
}
