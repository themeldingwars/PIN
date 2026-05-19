namespace GameServer.StaticDB.Records.customdata;

public record ConsumeItemCommandDef : ICommandDef
{
    public uint Id { get; set; }
}