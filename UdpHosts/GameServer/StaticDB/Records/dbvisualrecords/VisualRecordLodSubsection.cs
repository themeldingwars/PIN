namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class VisualRecordLodSubsection
{
    public uint MaterialId { get; set; }
    public uint VisaulRecordLodId { get; set; }
    public uint VisaulRecordLodSubsectionId { get; set; }
    public uint CziMapId { get; set; }
    public byte Order { get; set; }
}
