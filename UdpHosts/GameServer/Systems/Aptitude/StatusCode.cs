namespace GameServer.Systems.Aptitude;

public enum StatusCode : ushort
{
    None = 0,

    Error = 1, // "I can't do that" - Seems like its for unintended failures / bad chains
    CooldownRequirementFailed = 2,
    TargetRequirementFailed = 3,
    GenericRequirementFailed = 4, // "I can't do that" - tfRequireEliteLevelCommand, tfShootingDurationCommand, tfRequireJumpedCommand, tfRequireProjectileSlopeCommand, tfRequireReloadCommand, tfRequireTookDamageCommand, tfAtetmptToCalldownVehicleCommand.
    DurationRequirementFailed = 5,

    Status6 = 6, // Special case for HasTargetsDurationCommand?
    Status7 = 7, // Some ability special case?

    EnergyRequirementFailed = 8,
    LevelRequirementFailed = 9,
    ResourceRequirementFailed = 10,
    RangeRequirementFailed = 11,
    SessionPowerRequirementFailed = 12,
    SuperChargeRequirementFailed = 13,

    PINError = 1000,
}