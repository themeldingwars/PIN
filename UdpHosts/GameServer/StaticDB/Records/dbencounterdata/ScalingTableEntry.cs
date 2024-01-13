namespace GameServer.Data.SDB.Records.dbencounterdata;
public record class ScalingTableEntry
{
    public float HealthScale { get; set; }
    public uint PlayerCount { get; set; }
    public uint ScalingTableId { get; set; }
    public float DamageScale { get; set; }
    public uint Id { get; set; }
}
