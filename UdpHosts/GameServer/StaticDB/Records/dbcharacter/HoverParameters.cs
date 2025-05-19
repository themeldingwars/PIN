namespace GameServer.Data.SDB.Records.dbcharacter;
public record class HoverParameters
{
    public float CameraSlowYBackwardSpeed { get; set; }
    public float ForwardThrust { get; set; }
    public float MaxAltitudeSpringiness { get; set; }
    public float CameraYForwardFast { get; set; }
    public float AirResistance { get; set; }
    public float CameraYBackwardFast { get; set; }
    public float CameraFastZSpeed { get; set; }
    public float BobAmplitude { get; set; }
    public float CameraAirTurbulenceMaxSpeed { get; set; }
    public float RiseThrust { get; set; }
    public float AirResistanceConstant { get; set; }
    public float MinAltitudeSpringiness { get; set; }
    public float LowerThrust { get; set; }
    public float CameraFastXSpeed { get; set; }
    public float Gravity { get; set; }
    public float AirResistanceLinearWeight { get; set; }
    public float CameraSlowXSpeed { get; set; }
    public float CameraSlowZSpeed { get; set; }
    public float CameraAirTurbulenceMinSpeed { get; set; }
    public float CameraZFast { get; set; }
    public float CameraFastYBackwardSpeed { get; set; }
    public float MaxAltitude { get; set; }
    public float CameraXFast { get; set; }
    public float MinAltitude { get; set; }
    public float CameraThrusterShake { get; set; }
    public float ReverseThrust { get; set; }
    public float CameraAirTurbulenceShake { get; set; }
    public float MaxSpeed { get; set; }
    public float CameraSlowYForwardSpeed { get; set; }
    public float AirResistanceQuadraticWeight { get; set; }
    public float CameraFastYForwardSpeed { get; set; }
    public float BobFrequency { get; set; }
    public float StrafeThrust { get; set; }
    public uint Id { get; set; }
    public byte UseHoverThrustOnGround { get; set; }
}