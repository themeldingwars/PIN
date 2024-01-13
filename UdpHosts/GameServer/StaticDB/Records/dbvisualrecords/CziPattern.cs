namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class CziPattern
{
    public uint NameId { get; set; }
    public uint DisplayFlags { get; set; }
    public uint CziTextureId { get; set; }
    public uint Id { get; set; }
    public uint CziAssetId { get; set; }
    public uint Style { get; set; }
    public uint TypeFlags { get; set; }
}
