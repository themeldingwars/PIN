namespace GameServer.StaticDB.Records.customdata;

public record SetObjectLifespanCommandDef : ICommandDef
{
    public uint Id { get; set; }
}