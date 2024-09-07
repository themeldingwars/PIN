namespace GameServer.Data.SDB.Records.customdata;

public record CreateAbilityObjectCommandDef : ICommandDef
{
    public uint Id { get; set; }
}