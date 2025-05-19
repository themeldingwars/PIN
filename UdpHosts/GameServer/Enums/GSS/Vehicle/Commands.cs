namespace GameServer.Enums.GSS.Vehicle;

// https://github.com/themeldingwars/Documentation/wiki/Messages-Vehicle#commands
internal enum Commands
{
    MovementInput = 83,
    MovementInputFake = 84,
    SinAcquire_Source = 85,
    ReceiveCollisionDamage = 86,
    ActivateAbility = 87,
    DeactivateAbility = 88,
    SetWaterLevelAndDesc = 89,
    SetEffectsFlag = 90
}