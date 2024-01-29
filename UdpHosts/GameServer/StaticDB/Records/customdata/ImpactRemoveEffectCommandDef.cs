namespace GameServer.Data.SDB.Records.customdata;
public record class ImpactRemoveEffectCommandDef
{
    public uint Id { get; set; }
    public uint? EffectId { get; set; }
    public bool? RemoveFromSelf { get; set; }
}