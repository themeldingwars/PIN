namespace GameServer.Data.SDB.Records.customdata;

public record ModifyDamageByTargetCommandDef : ICommandDef
{
    public uint Id { get; set; }
}