namespace GameServer.StaticDB.Records.customdata;

public record ReplenishEffectDurationCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint? EffectId { get; set; }
}