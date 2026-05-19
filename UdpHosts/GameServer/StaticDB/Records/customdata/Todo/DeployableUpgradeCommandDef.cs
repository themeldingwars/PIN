namespace GameServer.StaticDB.Records.customdata;

public record DeployableUpgradeCommandDef : ICommandDef
{
    public uint Id { get; set; }
}