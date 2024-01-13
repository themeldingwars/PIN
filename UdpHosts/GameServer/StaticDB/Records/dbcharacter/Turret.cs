namespace GameServer.Data.SDB.Records.dbcharacter;
public record class Turret
{
    // public Vec3 BaseOffset { get; set; }
    // public Vec3 CameraOffset { get; set; }
    public uint Flags { get; set; }
    public string Behavior { get; set; }
    public uint Visualrec { get; set; }
    public float MinPitch { get; set; }
    public float MinYaw { get; set; }
    public float CameraFov { get; set; }
    public float MaxPitch { get; set; }
    public uint ExitRequestedAbility { get; set; }
    public float MaxYaw { get; set; }
    public string Name { get; set; }
    public uint Id { get; set; }
    public float MaxRotationRate { get; set; }
    public byte AoeWeapon { get; set; }
    public byte AttackType { get; set; }
    public byte Posture { get; set; }
}
