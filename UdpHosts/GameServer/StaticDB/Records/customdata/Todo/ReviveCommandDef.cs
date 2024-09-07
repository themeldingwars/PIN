namespace GameServer.Data.SDB.Records.customdata;

public record ReviveCommandDef : ICommandDef
{
    public uint Id { get; set; }
}