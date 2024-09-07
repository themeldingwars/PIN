namespace GameServer.Data.SDB.Records.customdata;

public record AddLootTableCommandDef : ICommandDef
{
    public uint Id { get; set; }
}