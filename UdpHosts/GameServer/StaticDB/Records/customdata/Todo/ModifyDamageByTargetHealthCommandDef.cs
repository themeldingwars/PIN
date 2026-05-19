namespace GameServer.StaticDB.Records.customdata;

public record ModifyDamageByTargetHealthCommandDef : ICommandDef
{
    public uint Id { get; set; }
}