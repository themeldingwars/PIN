namespace GameServer.Data.SDB.Records.aptfs;
public record class ConsumeEnergyCommandDef : ICommandDef
{
    public float Amount { get; set; }
    public uint Id { get; set; }
    public byte AllowOvercharge { get; set; }
    public byte OnTargets { get; set; }
    public byte AmountRegop { get; set; }
    public byte AllowPrediction { get; set; }
}