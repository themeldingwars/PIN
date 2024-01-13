namespace GameServer.Data.SDB.Records.apt;
public record class AimRangeDurationCommandDef
{
    public float Minslope { get; set; }
    public float Range { get; set; }
    public float Maxslope { get; set; }
    public uint Id { get; set; }
    public byte StaticOnly { get; set; }
    public byte FailNotStatic { get; set; }
    public byte Aimdown { get; set; }
    public byte YieldOnFail { get; set; }
    public byte Negate { get; set; }
    public byte Negateslope { get; set; }
}