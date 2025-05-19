namespace GameServer.Data.SDB.Records.vcs;
public record class DropPodComponentDef
{
    public string HardpointPrefix { get; set; }
    public uint LaunchStaggerTimems { get; set; }
    public uint RaiseShieldTimems { get; set; }
    public uint LowerShieldTimems { get; set; }
    public uint DeployableType { get; set; }
    public uint TurretDoorEffectId { get; set; }
    public uint PoseFile { get; set; }
    public uint NumHardpoints { get; set; }
    public uint PodBayEffectId { get; set; }
    public uint LaunchEffectId { get; set; }
    public uint Id { get; set; }
}