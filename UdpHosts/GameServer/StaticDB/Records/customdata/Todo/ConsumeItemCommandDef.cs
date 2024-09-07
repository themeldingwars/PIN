namespace GameServer.Data.SDB.Records.customdata;

public record ConsumeItemCommandDef : ICommandDef
{
    public uint Id { get; set; }
}