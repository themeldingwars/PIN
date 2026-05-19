namespace GameServer.StaticDB.Records.customdata;

public record RemoveEffectByTagCommandDef : ICommandDef
{
    public uint Id { get; set; }
}