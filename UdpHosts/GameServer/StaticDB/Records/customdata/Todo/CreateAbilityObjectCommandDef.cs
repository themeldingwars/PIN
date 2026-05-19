namespace GameServer.StaticDB.Records.customdata;

public record CreateAbilityObjectCommandDef : ICommandDef
{
    public uint Id { get; set; }
}