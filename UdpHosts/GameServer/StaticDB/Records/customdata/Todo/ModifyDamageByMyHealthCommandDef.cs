namespace GameServer.StaticDB.Records.customdata;

public record ModifyDamageByMyHealthCommandDef : ICommandDef
{
    public uint Id { get; set; }
}