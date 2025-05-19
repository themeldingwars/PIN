namespace GameServer.Data.SDB.Records.dblocalization;
public record class Profanity
{
    public string Swear { get; set; }
    public uint Id { get; set; }
    public uint Properties { get; set; }
    public uint Language { get; set; }
}