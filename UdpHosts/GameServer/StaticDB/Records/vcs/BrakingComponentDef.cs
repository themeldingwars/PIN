namespace GameServer.Data.SDB.Records.vcs;
public record class BrakingComponentDef
{
    public float MaxBreakingTorque { get; set; }
    public float MinPedalToBlock { get; set; }
    public float MinTimeToBlock { get; set; }
    public uint Id { get; set; }
}