namespace GameServer.Data.SDB.Records.dbelitelevels;
public record class EliteLevelsUpgradeTable
{
    public uint NameId { get; set; }
    public uint Special { get; set; }
    public uint DescriptionId { get; set; }
    public uint ItemAttribute { get; set; }
    public uint TableId { get; set; }
    public uint AttributeCategory { get; set; }
    public uint FormatSpecifierId { get; set; }
    public string Name { get; set; }
    public uint Id { get; set; }
    public byte InfiniteLevels { get; set; }
    public byte NegativeBoost { get; set; }
}