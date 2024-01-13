namespace GameServer.Data.SDB.Records.vcs;
public record class SteeringComponentDef
{
    public float MaxSpeedFullTilt { get; set; }
    public float MaxSteeringAngle { get; set; }
    public float MaxSpeedFullScalar { get; set; }
    public float VisualSteerScalar { get; set; }
    public uint Id { get; set; }
}
