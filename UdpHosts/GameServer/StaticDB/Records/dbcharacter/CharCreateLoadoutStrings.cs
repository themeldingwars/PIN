namespace GameServer.Data.SDB.Records.dbcharacter;
public record class CharCreateLoadoutStrings
{
    public uint LoadoutId { get; set; }
    public uint StringId { get; set; }
    public byte Level { get; set; }
}
