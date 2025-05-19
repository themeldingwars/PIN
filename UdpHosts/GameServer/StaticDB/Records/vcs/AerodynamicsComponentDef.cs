namespace GameServer.Data.SDB.Records.vcs;
public record class AerodynamicsComponentDef
{
    // public Vec3 ExtraGravity { get; set; }
    public float DragCoefficient { get; set; }
    public float CollisionThreshold { get; set; }
    public float AirDensity { get; set; }
    public float CollisionSpinDamping { get; set; }
    public float LowSpeedThreshold { get; set; }
    public float LowSpeedExtraDamping { get; set; }
    public float NormalSpinDamping { get; set; }
    public float LiftCoefficient { get; set; }
    public float FrontalArea { get; set; }
    public uint Id { get; set; }
}