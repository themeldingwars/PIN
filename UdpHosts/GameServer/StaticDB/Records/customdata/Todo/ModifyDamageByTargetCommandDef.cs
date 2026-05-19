namespace GameServer.StaticDB.Records.customdata;

public record ModifyDamageByTargetCommandDef : ICommandDef
{
    public uint Id { get; set; }
}