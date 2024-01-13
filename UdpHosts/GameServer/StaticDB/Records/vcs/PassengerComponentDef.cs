namespace GameServer.Data.SDB.Records.vcs;
public record class PassengerComponentDef
{
    // public Vec3 PassengerPoseFileOffset { get; set; }
    public string HardpointPrefix { get; set; }
    public uint MaxPassengers { get; set; }
    public float EjectionForce { get; set; }
    public uint EjectionEffectId { get; set; }
    public uint VisualRecord { get; set; }
    public float EjectionElevation { get; set; }
    public uint PassengerPoseFile { get; set; }
    public string Hardpoint { get; set; }
    public uint BoardingEffectId { get; set; }
    public uint Id { get; set; }
    public byte LeadingZero { get; set; }
    public byte ActivePassenger { get; set; }
    public byte Posture { get; set; }
    public byte ZeroBased { get; set; }
}
