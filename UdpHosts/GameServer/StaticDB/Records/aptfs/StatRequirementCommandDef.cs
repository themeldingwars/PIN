namespace GameServer.Data.SDB.Records.aptfs;
public record class StatRequirementCommandDef : ICommandDef
{
    public float Value { get; set; }
    public uint Id { get; set; }
    public ushort Stat1 { get; set; }
    public ushort Stat2 { get; set; }
    public byte Lessthan { get; set; }
    public byte Greaterthan { get; set; }
    public byte Equalto { get; set; }
    public byte ValueRegop { get; set; }
}
