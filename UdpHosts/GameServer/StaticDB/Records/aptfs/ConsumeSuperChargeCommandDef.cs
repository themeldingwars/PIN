namespace GameServer.Data.SDB.Records.aptfs;
public record class ConsumeSuperChargeCommandDef
{
    public float Percent { get; set; }
    public uint Id { get; set; }
    public byte AllowOvercharge { get; set; }
    public byte IsGainEvent { get; set; }
    public byte PercentRegop { get; set; }
}
