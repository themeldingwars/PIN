namespace GameServer.Data.SDB.Records.customdata;

public record DeployableUpgradeCommandDef : ICommandDef
{
    public uint Id { get; set; }
}