namespace GameServer.StaticDB.Records.customdata;

public record ModifyDamageByFactionCommandDef : ICommandDef
{
    public uint Id { get; set; }
}