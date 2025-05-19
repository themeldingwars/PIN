namespace GameServer.Enums.GSS.Deployable;

// https://github.com/themeldingwars/Documentation/wiki/Messages-Deployable#events
internal enum Events
{
    PartialUpdate = 1, // Delete
    KeyFrame = 4, // Delete
    TookHit = 83,
    AbilityProjectileFired = 84,
    PublicCombatLog = 85
}