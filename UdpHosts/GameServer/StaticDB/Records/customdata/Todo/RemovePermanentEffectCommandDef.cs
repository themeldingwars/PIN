namespace GameServer.Data.SDB.Records.customdata;

public record RemovePermanentEffectCommandDef : ICommandDef
{
    public uint Id { get; set; }
}