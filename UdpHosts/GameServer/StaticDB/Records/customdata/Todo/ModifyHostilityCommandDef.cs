namespace GameServer.Data.SDB.Records.customdata;

public record ModifyHostilityCommandDef : ICommandDef
{
    public uint Id { get; set; }
}