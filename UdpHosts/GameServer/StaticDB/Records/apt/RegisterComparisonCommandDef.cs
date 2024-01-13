namespace GameServer.Data.SDB.Records.apt;
public record class RegisterComparisonCommandDef
{
    public float CompareVal { get; set; }
    public float EqualTol { get; set; }
    public uint Id { get; set; }
    public byte EqualTo { get; set; }
    public byte LessThan { get; set; }
    public byte GreaterThan { get; set; }
}