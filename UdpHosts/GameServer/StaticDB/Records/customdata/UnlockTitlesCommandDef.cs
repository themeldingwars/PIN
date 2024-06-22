namespace GameServer.Data.SDB.Records.customdata;

public record UnlockTitlesCommandDef
{
    public uint Id { get; set; }
    public uint? TitleId { get; set; }
}