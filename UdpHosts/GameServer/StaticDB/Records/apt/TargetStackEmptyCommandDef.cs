namespace GameServer.StaticDB.Records.apt;
public record class TargetStackEmptyCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte NotEmpty { get; set; }
}