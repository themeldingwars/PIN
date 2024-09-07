namespace GameServer.Data.SDB.Records.customdata;

public record SetDefaultDamageBonusCommandDef : ICommandDef
{
    public uint Id { get; set; }
}