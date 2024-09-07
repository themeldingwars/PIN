namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireEnergyByRangeCommandDef : ICommandDef
{
    public float MinRange { get; set; }
    public float MaxRange { get; set; }
    public float MinEnergy { get; set; }
    public float MaxEnergy { get; set; }
    public uint Id { get; set; }
    public byte AmountRegop { get; set; }
    public byte AlsoConsume { get; set; }
    public byte AllowPrediction { get; set; }
}
