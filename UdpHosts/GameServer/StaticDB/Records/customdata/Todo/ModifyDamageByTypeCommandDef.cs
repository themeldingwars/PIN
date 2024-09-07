namespace GameServer.Data.SDB.Records.customdata;

public record ModifyDamageByTypeCommandDef : ICommandDef
{
    public uint Id { get; set; }
}