namespace GameServer.Data.SDB.Records.dbzonemetadata;
public record class PvPZoneInfo
{
    public uint MatchId { get; set; }
    public uint Id { get; set; }
    public uint DisplayedName { get; set; }
    public uint DisplayedType { get; set; }
}
