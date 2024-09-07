namespace GameServer.Data.SDB.Records.customdata;

public record ModifyDamageByHeadshotCommandDef : ICommandDef
{
    public uint Id { get; set; }
}