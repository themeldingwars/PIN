namespace GameServer.Data.SDB.Records.dbcharacter;
public record class CalendarSchedule
{
    public uint SecondsEnd { get; set; }
    public uint SecondsStart { get; set; }
    public string Value { get; set; }
    public int Amount { get; set; }
    public uint CalendarId { get; set; }
    public string Keyname { get; set; }
    public byte ValueType { get; set; }
}
