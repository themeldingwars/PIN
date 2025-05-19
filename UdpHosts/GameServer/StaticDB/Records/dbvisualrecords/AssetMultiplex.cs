namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class AssetMultiplex
{
    public uint AssetType { get; set; }
    public uint PropertyType { get; set; }
    public uint CompareMode { get; set; }
    public uint Id { get; set; }
}