namespace GameServer.Data.SDB.Records.apt;
public record class TimedActivationCommandDef
{
    public uint Id { get; set; }
    public uint Duration { get; set; }
    public byte CancelOnMove { get; set; }
}