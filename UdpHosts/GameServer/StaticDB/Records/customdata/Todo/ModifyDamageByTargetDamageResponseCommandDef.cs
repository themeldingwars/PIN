namespace GameServer.StaticDB.Records.customdata;

public record ModifyDamageByTargetDamageResponseCommandDef : ICommandDef
{
    public uint Id { get; set; }
}