namespace GameServer.Data.SDB.Records.dbcharacter;
public record class MonsterTitle
{
    public uint Id { get; set; }
    public uint PfxAssetId { get; set; }
    public uint LocalizedNameId { get; set; }
}