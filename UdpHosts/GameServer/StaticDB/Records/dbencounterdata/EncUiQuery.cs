namespace GameServer.Data.SDB.Records.dbencounterdata;
public record class EncUiQuery
{
    public uint LocalizedText { get; set; }
    public string UiType { get; set; }
    public uint LocalizedTitle { get; set; }
    public uint Id { get; set; }
    public byte Retval { get; set; }
}