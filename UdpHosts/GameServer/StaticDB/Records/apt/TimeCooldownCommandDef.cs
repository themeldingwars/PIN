namespace GameServer.Data.SDB.Records.apt;
public record class TimeCooldownCommandDef : ICommandDef
{
    public uint Duration { get; set; }
    public uint Category { get; set; }
    public uint Id { get; set; }
    public byte CheckCategory { get; set; }
    public byte CheckGlobal { get; set; }
    public byte CheckLocal { get; set; }
}