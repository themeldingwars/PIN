namespace GameServer.Data.SDB.Records.dblocalization;
public record class SupportedLanguages
{
    public string CodeName { get; set; }
    public string FullName { get; set; }
    public uint Id { get; set; }
}