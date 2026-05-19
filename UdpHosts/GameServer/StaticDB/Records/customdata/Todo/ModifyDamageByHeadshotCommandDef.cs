namespace GameServer.StaticDB.Records.customdata;

public record ModifyDamageByHeadshotCommandDef : ICommandDef
{
    public uint Id { get; set; }
}