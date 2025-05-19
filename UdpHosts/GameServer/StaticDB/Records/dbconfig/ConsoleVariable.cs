namespace GameServer.Data.SDB.Records.dbconfig;
public record class ConsoleVariable
{
    public string Value { get; set; }
    public string Name { get; set; }
    public uint Id { get; set; }
    public byte Serverside { get; set; }
}