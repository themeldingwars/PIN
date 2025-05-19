namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class AssetOverrideGroupEntry
{
    public uint AssetoverridegroupId { get; set; }
    public uint OverrideAssetId { get; set; }
    public uint TargetAssetId { get; set; }
}