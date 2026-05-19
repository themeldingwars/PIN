namespace GameServer.StaticDB.Records.customdata;

public record SetOrientationCommandDef : ICommandDef
{
    public uint Id { get; set; }
}