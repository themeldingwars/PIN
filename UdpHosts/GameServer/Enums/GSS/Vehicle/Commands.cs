namespace GameServer.Enums.GSS.Vehicle;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "https://github.com/themeldingwars/Documentation/wiki/Messages-Vehicle#commands")]
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