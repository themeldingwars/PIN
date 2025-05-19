namespace GameServer.Data.SDB.Records.dbencounterdata;
public record class EncUiQueryInput
{
    public uint QueryId { get; set; }
    public string Name { get; set; }
    public uint DefaultVal { get; set; }
    public uint Id { get; set; }
    public uint UserDefined { get; set; }
}