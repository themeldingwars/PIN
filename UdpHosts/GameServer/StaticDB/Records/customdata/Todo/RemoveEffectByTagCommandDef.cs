namespace GameServer.Data.SDB.Records.customdata;

public record RemoveEffectByTagCommandDef : ICommandDef
{
    public uint Id { get; set; }
}