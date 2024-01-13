namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireResourceCommandDef
{
    public int Amount { get; set; }
    public uint ResourceSdbId { get; set; }
    public uint Id { get; set; }
    public byte Behavior { get; set; }
    public byte ApplyToPlayer { get; set; }
    public byte ApplyToArmy { get; set; }
}
