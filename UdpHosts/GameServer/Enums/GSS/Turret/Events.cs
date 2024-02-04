using System.Diagnostics.CodeAnalysis;

namespace GameServer.Enums.GSS.Turret;

[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "https://github.com/themeldingwars/Documentation/wiki/Messages-Turret#events")]
internal enum Events
{
    PartialUpdate = 1, // Delete
    KeyFrame = 4, // Delete
    WeaponProjectileFired = 83
}