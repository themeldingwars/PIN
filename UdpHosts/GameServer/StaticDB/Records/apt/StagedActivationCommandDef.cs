namespace GameServer.Data.SDB.Records.apt;
public record class StagedActivationCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint SelfEffectId { get; set; }
    public byte PassRegister { get; set; }
    public byte PassBonus { get; set; }
    public byte AllowPrediction { get; set; }
}