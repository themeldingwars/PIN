namespace GameServer.Data.SDB.Records.customdata;

public record ModifyDamageByTargetHealthCommandDef : ICommandDef
{
    public uint Id { get; set; }
}