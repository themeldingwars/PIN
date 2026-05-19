namespace GameServer.StaticDB.Records.customdata;

public record ModifyDamageByTypeCommandDef : ICommandDef
{
    public uint Id { get; set; }
}