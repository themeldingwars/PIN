using System.Diagnostics.CodeAnalysis;

namespace GameServer.Enums;

[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "TODO")]

// Sourced from 1189 commondb.sdb
public enum AptitudeStatOp : ushort
{
    Addend = 1,
    Multiplier = 2,
}