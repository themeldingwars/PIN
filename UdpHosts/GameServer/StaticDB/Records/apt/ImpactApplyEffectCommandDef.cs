namespace GameServer.Data.SDB.Records.apt;
public record class ImpactApplyEffectCommandDef
{
    public uint EffectId { get; set; }
    public uint Id { get; set; }
    public byte PassTargets { get; set; }
    public byte ApplyToSelf { get; set; }
    public byte OverrideInitiator { get; set; }
    public byte UseFormer { get; set; }
    public byte RemoveOnRollback { get; set; }
    public byte PassRegister { get; set; }
    public byte OverrideInitiatorWithTarget { get; set; }
    public byte PassBonus { get; set; }
    public byte InheritInitPos { get; set; }
    public byte AllowPrediction { get; set; }
}