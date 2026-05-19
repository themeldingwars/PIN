namespace GameServer.Systems.Aptitude;

public enum StatusCode : ushort
{
    None = 0,

    Status1 = 1, // Seems like its for unintended failures / bad chains
    Status2_CooldownFail = 2,
    Status3_TargetingFail = 3,
    Status4 = 4,
    Status5_DurationFail = 5,
    Status6 = 6,
    Status7 = 7,
    Status8 = 8,
    Status9 = 9,
    Status10 = 10,
    Status11 = 11,

    PINError = 1000,
}