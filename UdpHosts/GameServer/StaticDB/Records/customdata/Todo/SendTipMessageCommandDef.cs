namespace GameServer.StaticDB.Records.customdata;

public record SendTipMessageCommandDef : ICommandDef
{
    public uint Id { get; set; }
}