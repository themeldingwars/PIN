namespace GameServer.Data.SDB.Records.dbcharacter;
public record class DamageType
{
    public uint LocalizedNameId { get; set; }
    public string Color { get; set; }
    public uint IconAssetId { get; set; }
    public string Name { get; set; }
    public ushort DamageReduction { get; set; }
    public byte Id { get; set; }
}