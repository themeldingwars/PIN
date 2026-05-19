namespace GameServer.StaticDB.Records.dbcharacter;
public record class HardpointTypeEntry
{
    public uint HardpointTypeId { get; set; }
    public uint DeployableTypeId { get; set; }
    public uint Id { get; set; }
}