namespace GameServer.StaticDB.Records.apt;
public record class CallCommandDef : ICommandDef
{
    public uint AbilityId { get; set; }
    public uint Id { get; set; }
}