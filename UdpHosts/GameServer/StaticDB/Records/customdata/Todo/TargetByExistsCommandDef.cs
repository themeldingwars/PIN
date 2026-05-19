namespace GameServer.StaticDB.Records.customdata;

public record TargetByExistsCommandDef : ICommandDef
{
    public uint Id { get; set; }
}