namespace GameServer.Enums.GSS.Vehicle;

// https://github.com/themeldingwars/Documentation/wiki/Messages-Vehicle#events
internal enum Events
{
    PartialUpdate = 1, // Delete
    KeyFrame = 4, // Delete
    AbilityActivated = 83,
    AbilityFailed = 84,
    PublicCombatLog = 85,
    CurrentPoseUpdate = 86,
    TookDebugWeaponHitPublic = 87,
    ForcedMovement = 88,
    ForcedMovementCancelled = 89,
    FlipPunch = 90,
    DebugMovementUpdate = 91
}