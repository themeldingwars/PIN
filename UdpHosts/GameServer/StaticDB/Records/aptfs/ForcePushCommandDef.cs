namespace GameServer.Data.SDB.Records.aptfs;
public record class ForcePushCommandDef
{
    public float Strength { get; set; }
    public float Falloff { get; set; }
    public float Loft { get; set; }
    public uint Id { get; set; }
    public byte ImpactPosition { get; set; }
    public byte DoAnimation { get; set; }
    public byte StrengthRegop { get; set; }
}
