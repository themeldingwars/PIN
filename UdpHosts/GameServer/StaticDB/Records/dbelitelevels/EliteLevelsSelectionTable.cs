namespace GameServer.Data.SDB.Records.dbelitelevels;
public record class EliteLevelsSelectionTable
{
    public uint UpgradeCategory { get; set; }
    public uint UpgradeId { get; set; }
    public uint TableId { get; set; }
    public uint Random { get; set; }
    public string Name { get; set; }
    public uint Id { get; set; }
}
