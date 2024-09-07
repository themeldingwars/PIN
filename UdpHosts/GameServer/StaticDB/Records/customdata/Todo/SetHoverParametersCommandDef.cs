namespace GameServer.Data.SDB.Records.customdata;

public record SetHoverParametersCommandDef : ICommandDef
{
    public uint Id { get; set; }
}