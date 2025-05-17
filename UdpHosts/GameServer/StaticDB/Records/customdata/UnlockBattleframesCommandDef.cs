namespace GameServer.Data.SDB.Records.customdata;

public record UnlockBattleframesCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint SdbId { get; set; } = 0;
}
