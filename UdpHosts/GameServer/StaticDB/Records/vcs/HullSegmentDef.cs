namespace GameServer.Data.SDB.Records.vcs;
public record class HullSegmentDef
{
    // public Vec3 CenterOfMass { get; set; }
    public uint LocalPoseFile { get; set; }
    public uint AnimationNetwork { get; set; }
    public uint RemotePoseFile { get; set; }
    public string EntryPoints { get; set; }
    public uint VisualRecord { get; set; }
    public float Friction { get; set; }
    public string ExitPoints { get; set; }
    public float ExitJumpStrength { get; set; }
    public float Restitution { get; set; }
    public uint Id { get; set; }
    public float Mass { get; set; }
    public byte AudioGroup { get; set; }
    public byte PivotCollision { get; set; }
}