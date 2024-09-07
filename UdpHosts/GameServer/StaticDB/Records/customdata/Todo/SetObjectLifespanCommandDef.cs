namespace GameServer.Data.SDB.Records.customdata;

public record SetObjectLifespanCommandDef : ICommandDef
{
    public uint Id { get; set; }
}