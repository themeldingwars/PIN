namespace GameServer.StaticDB.Records.customdata;

public record RemovePermanentEffectCommandDef : ICommandDef
{
    public uint Id { get; set; }
}