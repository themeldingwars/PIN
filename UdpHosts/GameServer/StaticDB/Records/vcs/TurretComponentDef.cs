namespace GameServer.Data.SDB.Records.vcs;
public record class TurretComponentDef
{
    // public Vec3 GunnerPoseFileOffset { get; set; }
    public uint VisualrecId { get; set; }
    public uint AnimnetId { get; set; }
    public uint GunnerPoseFile { get; set; }
    public string Hardpoint { get; set; }
    public uint TurretType { get; set; }
    public uint Id { get; set; }
    public byte DriverOperated { get; set; }
    public byte Posture { get; set; }
}
