namespace GameServer.StaticDB.Records.customdata;
public record class ImpactRemoveEffectCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint? EffectId { get; set; }
    public bool? RemoveFromSelf { get; set; }
}