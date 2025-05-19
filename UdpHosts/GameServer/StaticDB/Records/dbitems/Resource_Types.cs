namespace GameServer.Data.SDB.Records.dbitems;
public record class Resource_Types
{
    public uint LocalizedNameId { get; set; }
    public uint ParentId { get; set; }
    public uint WebIconId { get; set; }
    public uint MarketSortOrder { get; set; }
    public float BlueprintQuantityMod { get; set; }
    public uint Id { get; set; }
    public byte MarketCategory { get; set; }
}