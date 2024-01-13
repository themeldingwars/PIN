namespace GameServer.Data.SDB.Records.aptfs;
public record class EnergyToDamageCommandDef
{
    public float MaxEnergyAllowed { get; set; }
    public float EnergyPerPoint { get; set; }
    public uint Id { get; set; }
    public float EnergyRequired { get; set; }
    public byte DamageType { get; set; }
}
