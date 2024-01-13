namespace GameServer.Data.SDB.Records.apt;
public record class InstantActivationCommandDef
{
    public uint CategoryCooldownPrecoolCount { get; set; }
    public uint CategoryCooldown { get; set; }
    public uint LocalCooldown { get; set; }
    public uint Category { get; set; }
    public uint LocalCooldownPrecoolCount { get; set; }
    public uint Id { get; set; }
    public uint GlobalCooldown { get; set; }
    public byte PrecoolRegop { get; set; }
    public byte DurationRegop { get; set; }
    public byte PreventReset { get; set; }
    public byte CategoryPrecoolRegop { get; set; }
}