namespace GameServer.Data.SDB.Records.dbencounterdata;
public record class EncUiQueryOption
{
    public uint LocalizedText { get; set; }
    public string EnumName { get; set; }
    public uint QueryId { get; set; }
    public uint Id { get; set; }
}