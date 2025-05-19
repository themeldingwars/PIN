namespace GameServer.Data.SDB.Records.vcs;
public record class HullUpgradeComponentDef
{
    public uint LocalPoseFile { get; set; }
    public uint AnimationNetwork { get; set; }
    public uint RemotePoseFile { get; set; }
    public uint VisualRecord { get; set; }
    public uint Id { get; set; }
}