namespace GameServer.Data.SDB.Records.vcs;
public record class BaseComponentDef
{
    public uint SdbGuid { get; set; }
    public uint ComponentId { get; set; }
    public uint Id { get; set; }
    public ushort VehicleId { get; set; }
}
