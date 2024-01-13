namespace GameServer.Data.SDB.Records.dbcharacter;
public record class FactionGroup
{
    public uint LocalizedNameId { get; set; }
    public uint JobBoardIconId { get; set; }
    public uint DescriptionId { get; set; }
    public uint Id { get; set; }
}
