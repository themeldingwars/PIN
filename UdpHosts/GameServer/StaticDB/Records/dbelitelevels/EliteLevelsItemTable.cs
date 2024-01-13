namespace GameServer.Data.SDB.Records.dbelitelevels;
public record class EliteLevelsItemTable
{
    public uint NameId { get; set; }
    public uint Type { get; set; }
    public uint Amount { get; set; }
    public string Name { get; set; }
    public uint SdbId { get; set; }
    public uint Id { get; set; }
}
