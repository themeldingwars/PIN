namespace GameServer.Data.SDB.Records.customdata;

public record ModifyDamageByTargetDamageResponseCommandDef : ICommandDef
{
    public uint Id { get; set; }
}