namespace GameServer.StaticDB.Records.apt;
public record class TimedActivationCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint Duration { get; set; }
    public byte CancelOnMove { get; set; }
}