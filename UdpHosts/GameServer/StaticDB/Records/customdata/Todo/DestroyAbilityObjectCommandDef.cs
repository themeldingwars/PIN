namespace GameServer.StaticDB.Records.customdata;

public record DestroyAbilityObjectCommandDef : ICommandDef
{
    public uint Id { get; set; }
}