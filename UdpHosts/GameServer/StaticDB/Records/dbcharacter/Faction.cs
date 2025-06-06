namespace GameServer.Data.SDB.Records.dbcharacter;
public record class Faction
{
    public uint LocalizedNameId { get; set; }
    public int StartingReputation { get; set; }
    public string InternalName { get; set; }
    public uint JobBoardIconId { get; set; }
    public uint DescriptionId { get; set; }
    public int MaxReputation { get; set; }
    public uint AbbreviatedName { get; set; }
    public int MinReputation { get; set; }
    public uint Id { get; set; }
    public byte DefaultStancePriority { get; set; }
    public sbyte DefaultStance { get; set; }
}