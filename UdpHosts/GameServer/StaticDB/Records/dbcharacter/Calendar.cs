namespace GameServer.Data.SDB.Records.dbcharacter;
public record class Calendar
{
    public uint SecondsEnd { get; set; }
    public uint SecondsStart { get; set; }
    public uint Id { get; set; }
    public byte RepeatMode { get; set; }
}
