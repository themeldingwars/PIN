namespace GameServer.Data.SDB.Records.customdata;

public record ActivateMissionCommandDef : ICommandDef
{
    public uint Id { get; set; }
}