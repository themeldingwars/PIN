namespace GameServer.Data.SDB.Records.customdata;

public record SendTipMessageCommandDef : ICommandDef
{
    public uint Id { get; set; }
}