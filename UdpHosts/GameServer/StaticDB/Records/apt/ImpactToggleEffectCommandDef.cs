namespace GameServer.Data.SDB.Records.apt;
public record class ImpactToggleEffectCommandDef
{
    public uint PreApplyChain { get; set; }
    public uint EffectId { get; set; }
    public uint Id { get; set; }
    public byte PassRegister { get; set; }
    public byte FailOnRemove { get; set; }
    public byte PassBonus { get; set; }
    public byte AllowPrediction { get; set; }
}