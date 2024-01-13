namespace GameServer.Data.SDB.Records.dbcharacter;
public record class GliderParameters
{
    public float CameraStallShake { get; set; }
    public float CameraTurnRoll { get; set; }
    public float CameraStallWarnWobbleAmp { get; set; }
    public float AirResistance { get; set; }
    public float CameraAirTurbulenceMaxSpeed { get; set; }
    public float DivePitchLowSpeed { get; set; }
    public float EnterStallSpeed { get; set; }
    public int DivePitchExponent { get; set; }
    public float CameraFastYSpeed { get; set; }
    public float CameraStallWarnWobbleFreq { get; set; }
    public float CameraStallWarnSpeedDiff { get; set; }
    public float CameraYFast { get; set; }
    public float AirResistanceLinearWeight { get; set; }
    public float DivePitchHigh { get; set; }
    public float DivePitchHighSpeed { get; set; }
    public float CameraAirTurbulenceMinSpeed { get; set; }
    public float CameraThrusterShake { get; set; }
    public float TurnRate { get; set; }
    public float ExitStallSpeed { get; set; }
    public float CameraStallWarnShake { get; set; }
    public float ThrustEnergyUsage { get; set; }
    public float CameraAirTurbulenceShake { get; set; }
    public float StallGravity { get; set; }
    public float ThrustAccel { get; set; }
    public float AirResistanceQuadraticWeight { get; set; }
    public float MaxGravity { get; set; }
    public float Efficiency { get; set; }
    public float DivePitchLow { get; set; }
    public float CameraSlowYSpeed { get; set; }
    public float ThrustMaxSpeed { get; set; }
    public float ThrustStartEnergyUsage { get; set; }
    public uint Id { get; set; }
    public byte PlaneMode { get; set; }
}
