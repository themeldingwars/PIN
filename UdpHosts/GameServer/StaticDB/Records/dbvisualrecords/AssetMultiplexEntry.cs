namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class AssetMultiplexEntry
{
    public uint RecurseMultiplexId { get; set; }
    public uint PropertyValue { get; set; }
    public uint AssetId { get; set; }
    public float Scale { get; set; }
    public string HardpointName { get; set; }
    public uint MultiplexId { get; set; }
    public byte Loop { get; set; }
    public byte HardkillRollback { get; set; }
    public byte IsDefault { get; set; }
}