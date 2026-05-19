namespace GameServer.StaticDB.Records.customdata;

public record TargetByNPCTypeCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public ushort Type { get; set; } = 0;
}