namespace GameServer.Data.SDB.Records.customdata;

public record TargetByExistsCommandDef : ICommandDef
{
    public uint Id { get; set; }
}