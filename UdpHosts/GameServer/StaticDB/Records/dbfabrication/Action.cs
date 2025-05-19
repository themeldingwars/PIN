namespace GameServer.Data.SDB.Records.dbfabrication;
public record class Action
{
    public uint NameId { get; set; }
    public uint Flags { get; set; }
    public uint DescriptionId { get; set; }
    public uint Id { get; set; }
}