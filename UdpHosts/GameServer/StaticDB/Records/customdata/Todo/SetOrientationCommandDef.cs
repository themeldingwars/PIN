namespace GameServer.Data.SDB.Records.customdata;

public record SetOrientationCommandDef : ICommandDef
{
    public uint Id { get; set; }
}