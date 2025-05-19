namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class NameCard
{
    public string Name { get; set; }
    public uint Id { get; set; }
    public uint TextureAssetId { get; set; }
}