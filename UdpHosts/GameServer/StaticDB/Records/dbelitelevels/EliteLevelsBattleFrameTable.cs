namespace GameServer.Data.SDB.Records.dbelitelevels;
public record class EliteLevelsBattleFrameTable
{
    public uint NameId { get; set; }
    public uint LoadoutId { get; set; }
    public uint Type { get; set; }
    public string Name { get; set; }
    public uint SdbId { get; set; }
    public uint Id { get; set; }
}
