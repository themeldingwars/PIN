namespace GameServer.Data.SDB.Records.aptfs;
public record class BullrushCommandDef : ICommandDef
{
    public uint ImpactEffect { get; set; }
    public uint Duration { get; set; }
    public float Speed { get; set; }
    public uint Id { get; set; }
    public byte SpeedRegop { get; set; }
    public byte DurationRegop { get; set; }
}
