namespace GameServer.Data.SDB.Records.vcs;
public record class EngineUpgradeComponentDef
{
    public float MaxTorqueMultiplier { get; set; }
    public float TopSpeedMultiplier { get; set; }
    public uint Id { get; set; }
}
