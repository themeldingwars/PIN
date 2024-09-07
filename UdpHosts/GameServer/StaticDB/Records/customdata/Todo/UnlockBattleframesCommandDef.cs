namespace GameServer.Data.SDB.Records.customdata;

public record UnlockBattleframesCommandDef : ICommandDef
{
    public uint Id { get; set; }
}