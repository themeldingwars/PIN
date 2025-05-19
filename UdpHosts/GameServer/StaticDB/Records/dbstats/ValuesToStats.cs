namespace GameServer.Data.SDB.Records.dbstats;
public record class ValuesToStats
{
    public uint StatId { get; set; }
    public uint Id { get; set; }
    public uint StatValueId { get; set; }
}