namespace GameServer.Enums.GSS.Deployable;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "https://github.com/themeldingwars/Documentation/wiki/Messages-Deployable#events")]
internal enum Events
{
    PartialUpdate = 1, // Delete
    KeyFrame = 4, // Delete
    TookHit = 83,
    AbilityProjectileFired = 84,
    PublicCombatLog = 85
}