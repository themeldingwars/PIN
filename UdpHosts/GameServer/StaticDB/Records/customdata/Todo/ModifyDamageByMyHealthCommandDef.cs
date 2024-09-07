namespace GameServer.Data.SDB.Records.customdata;

public record ModifyDamageByMyHealthCommandDef : ICommandDef
{
    public uint Id { get; set; }
}