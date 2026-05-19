namespace GameServer.StaticDB.Records.customdata;

public record SetPoweredStateCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public bool? PowerOn { get; set; } = null;
}