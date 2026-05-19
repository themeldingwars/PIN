namespace GameServer.StaticDB.Records.dbitems;
public record class LockBoxKey
{
    public uint Id { get; set; }
    public ushort ConsumptionPriority { get; set; }
}