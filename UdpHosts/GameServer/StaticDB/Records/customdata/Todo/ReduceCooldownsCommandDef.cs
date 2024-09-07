namespace GameServer.Data.SDB.Records.customdata;

public record ReduceCooldownsCommandDef : ICommandDef
{
    public uint Id { get; set; }
}