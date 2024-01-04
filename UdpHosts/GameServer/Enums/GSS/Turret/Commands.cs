namespace GameServer.Enums.GSS.Turret;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "https://github.com/themeldingwars/Documentation/wiki/Messages-Turret#commands")]
internal enum Commands
{
    PoseUpdate = 83,
    FireBurst = 84,
    FireEnd = 85,
    FireWeaponProjectile = 86,
    ReportProjectileHit = 87
}