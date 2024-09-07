namespace GameServer.Data.SDB.Records.customdata;
public record class DeployableSpawnCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint? DeployableTypeId { get; set; }
    public uint? Lifetime { get; set; }
}