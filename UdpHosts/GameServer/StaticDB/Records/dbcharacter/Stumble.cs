namespace GameServer.Data.SDB.Records.dbcharacter;
public record class Stumble
{
    public uint AnimIndex { get; set; }
    public uint StatusfxId { get; set; }
    public uint Duration { get; set; }
    public uint CooldownMs { get; set; }
    public float Distance { get; set; }
    public uint Id { get; set; }
    public byte OnlyOnce { get; set; }
}
