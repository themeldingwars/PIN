namespace GameServer.Data.SDB.Records.dbcharacter;
public record class Head
{
    public uint LocNameId { get; set; }
    public uint VisualrecId { get; set; }
    public uint AnimnetId { get; set; }
    public uint HeadId { get; set; }
    public string Sex { get; set; }
    public byte UnlockedByDefault { get; set; }
    public byte RaceId { get; set; }
    public byte PlayerSelectable { get; set; }
    public byte ExcludeFromRelease { get; set; }
}