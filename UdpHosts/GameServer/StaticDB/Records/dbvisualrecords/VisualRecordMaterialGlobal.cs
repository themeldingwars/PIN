namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class VisualRecordMaterialGlobal
{
    public uint MaterialGlobalId { get; set; }
    public uint VisaulRecordLodSubsectionId { get; set; }
    public string Value { get; set; }
}
