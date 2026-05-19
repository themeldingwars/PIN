namespace GameServer.StaticDB.Records.aptfs;
public record class ApplyFreezeCommandDef : ICommandDef
{
    public uint Id { get; set; }
}