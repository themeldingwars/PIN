namespace GameServer.Data.SDB.Records.customdata;

public record AwardRedBeansCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint Amount { get; set; }
}