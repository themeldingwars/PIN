namespace GameServer.Data.SDB.Records.aptfs;
public record class StatModifierCommandDef
{
    public float Value { get; set; }
    public uint Id { get; set; }
    public ushort Stat { get; set; }
    public byte Permanent { get; set; }
    public byte Op { get; set; }
    public byte ValueRegop { get; set; }
}
