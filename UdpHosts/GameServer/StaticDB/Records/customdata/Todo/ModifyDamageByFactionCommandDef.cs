namespace GameServer.Data.SDB.Records.customdata;

public record ModifyDamageByFactionCommandDef : ICommandDef
{
    public uint Id { get; set; }
}