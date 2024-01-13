namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class VisualRecordLod
{
    public uint MeshAssetId { get; set; }
    public uint VisaulRecordLodId { get; set; }
    public uint VisaulRecordId { get; set; }
    public uint Id { get; set; }
    public byte NoCull { get; set; }
    public byte Order { get; set; }
}
