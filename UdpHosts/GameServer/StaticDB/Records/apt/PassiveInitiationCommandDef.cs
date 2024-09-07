namespace GameServer.Data.SDB.Records.apt;
public record class PassiveInitiationCommandDef : ICommandDef
{
    public uint InitiationInterval { get; set; }
    public uint Id { get; set; }
}