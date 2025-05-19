namespace GameServer.Data.SDB.Records.apt;
public record class CommandType
{
    public string Tblname { get; set; }
    public string Environment { get; set; }
    public string SdbFullname { get; set; }
    public uint Id { get; set; }
}