namespace GameServer.StaticDB.Records.customdata;

public record ActivateMissionCommandDef : ICommandDef
{
    public uint Id { get; set; }
}