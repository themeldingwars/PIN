namespace GameServer.Data.SDB.Records.customdata;

public record SetPoweredStateCommandDef : ICommandDef
{
    public uint Id { get; set; }
}