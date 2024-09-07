namespace GameServer.Data.SDB.Records.customdata;

public record DestroyAbilityObjectCommandDef : ICommandDef
{
    public uint Id { get; set; }
}