namespace GameServer.Data.SDB.Records.dblocalization;
public record class NameGeneratorOrderMap
{
    public uint SuffixId { get; set; }
    public uint PrefixId { get; set; }
    public uint LanguageId { get; set; }
}
