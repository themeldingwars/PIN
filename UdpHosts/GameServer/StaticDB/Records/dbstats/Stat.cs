namespace GameServer.Data.SDB.Records.dbstats;
public record class Stat
{
    public uint NameId { get; set; }
    public uint DescriptionId { get; set; }
    public string Name { get; set; }
    public uint GameTypes { get; set; }
    public uint Id { get; set; }
    public uint GroupTypeId { get; set; }
}