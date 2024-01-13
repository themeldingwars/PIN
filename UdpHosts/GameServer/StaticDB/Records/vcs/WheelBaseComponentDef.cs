namespace GameServer.Data.SDB.Records.vcs;
public record class WheelBaseComponentDef
{
    public float SlipAngle { get; set; }
    public float TorqueRollFactor { get; set; }
    public uint LeftFlags { get; set; }
    public float UnitInertiaYaw { get; set; }
    public float NosteerWheelFriction { get; set; }
    public float ForceFeedbackScalar { get; set; }
    public float TorquePitchFactor { get; set; }
    public uint WheelMap3 { get; set; }
    public uint RightFlags { get; set; }
    public float WheelMass { get; set; }
    public uint WheelAssetId3 { get; set; }
    public float TorqueYawFactor { get; set; }
    public uint FrontFlags { get; set; }
    public uint WheelAssetId1 { get; set; }
    public uint SteerFlags { get; set; }
    public float FrictionEqualizer { get; set; }
    public uint BrakeFlags { get; set; }
    public uint DustFlags { get; set; }
    public uint MarkingFlags { get; set; }
    public float UnitInertiaPitch { get; set; }
    public float ExtraTorqueFactor { get; set; }
    public float UnitInertiaRoll { get; set; }
    public uint WheelAssetId2 { get; set; }
    public float MaxVelPosFriction { get; set; }
    public uint WheelMap1 { get; set; }
    public uint RearFlags { get; set; }
    public float MaxSpeed { get; set; }
    public float SteerWheelFriction { get; set; }
    public uint WheelMap2 { get; set; }
    public float ViscosityFriction { get; set; }
    public uint Id { get; set; }
    public uint RotateFlags { get; set; }
    public byte AnimTiltsWheels { get; set; }
    public byte AnimRepositionsWheels { get; set; }
    public byte OnlyStaticCollision { get; set; }
}
