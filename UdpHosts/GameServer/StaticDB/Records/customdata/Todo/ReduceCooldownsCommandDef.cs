namespace GameServer.StaticDB.Records.customdata;

public record ReduceCooldownsCommandDef : ICommandDef
{
    public uint Id { get; set; }
}