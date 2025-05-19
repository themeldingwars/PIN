namespace GameServer.Data.SDB.Records.dbelitelevels;
public record class EliteLevelsTableEntries
{
    public uint TableId { get; set; }
    public float Bonus { get; set; }
    public uint Id { get; set; }
    public uint UpgradeLevel { get; set; }
}