namespace GameServer.StaticDB.Records.customdata;

public record ModifyHostilityCommandDef : ICommandDef
{
    public uint Id { get; set; }
}