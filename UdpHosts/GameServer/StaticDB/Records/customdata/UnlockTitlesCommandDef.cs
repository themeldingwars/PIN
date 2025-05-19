namespace GameServer.Data.SDB.Records.customdata;

public record UnlockTitlesCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint TitleId { get; set; }
}