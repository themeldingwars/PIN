namespace GameServer.Data.SDB.Records.customdata;

public record CarryableObjectSpawnCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint? CarryableTypeId { get; set; }
    public uint? Lifetime { get; set; }
}