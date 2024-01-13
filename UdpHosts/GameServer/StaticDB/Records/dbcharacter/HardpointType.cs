namespace GameServer.Data.SDB.Records.dbcharacter;
public record class HardpointType
{
    public uint Flags { get; set; }
    public uint CooldownDuration { get; set; }
    public uint ReadyStatusfxId { get; set; }
    public uint FilledStatusfxId { get; set; }
    public float PowerRequired { get; set; }
    public string Name { get; set; }
    public uint CooldownStatusfxId { get; set; }
    public uint Id { get; set; }
    public uint InteractableStatusfxId { get; set; }
}
