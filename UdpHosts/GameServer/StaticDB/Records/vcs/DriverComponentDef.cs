using FauFau.Util.CommmonDataTypes;

namespace GameServer.Data.SDB.Records.vcs;

public record class DriverComponentDef
{
    public Vector3 DriverPoseFileOffset { get; set; }
    public uint CockpitVisualrec { get; set; }
    public uint DriverPoseFile { get; set; }
    public string Hardpoint { get; set; }
    public uint Id { get; set; }
    public byte Posture { get; set; }
}