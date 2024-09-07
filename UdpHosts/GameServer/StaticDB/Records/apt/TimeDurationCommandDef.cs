namespace GameServer.Data.SDB.Records.apt;
public record class TimeDurationCommandDef : ICommandDef
{
    public uint DurationMs { get; set; }
    public uint Id { get; set; }
    public byte DurationRegop { get; set; }
    public byte Negate { get; set; }
}