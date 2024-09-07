namespace GameServer.Data.SDB.Records.customdata;

public record RegisterTimedTriggerCommandDef : ICommandDef
{
    public uint Id { get; set; }
}