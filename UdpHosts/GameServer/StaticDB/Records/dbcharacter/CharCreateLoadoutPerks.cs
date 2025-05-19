namespace GameServer.Data.SDB.Records.dbcharacter;
public record class CharCreateLoadoutPerks
{
    public uint LoadoutId { get; set; }
    public uint PerkItemId { get; set; }
    public byte Level { get; set; }
}