namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class OrnamentsMap
{
    public uint OrnamentRecordId { get; set; }
    public uint SexFlags { get; set; }
    public uint RaceFlags { get; set; }
    public uint VisualRecordId { get; set; }
    public uint GroupId { get; set; }
    public byte VisualsGroupType { get; set; }
}