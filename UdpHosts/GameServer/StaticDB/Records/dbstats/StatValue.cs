namespace GameServer.Data.SDB.Records.dbstats;
public record class StatValue
{
    public uint DataType { get; set; }
    public float FMaxValue { get; set; }
    public float FDefault { get; set; }
    public uint IMaxValue { get; set; }
    public uint IMinValue { get; set; }
    public uint IDefault { get; set; }
    public float FMinValue { get; set; }
    public string Name { get; set; }
    public uint Id { get; set; }
}
