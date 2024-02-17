using System.Diagnostics.CodeAnalysis;

namespace GameServer.Enums;

[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "TODO")]
public enum Operand : byte
{
    ASSIGN = 0,
    ADDITIVE = 1,
    MULTIPLICATIVE = 2,
    PERK_DAMAGE_SCALAR = 3, // Uncertain
    DIVIDE_FIRST_BY_SECOND = 4, // Uncertain
    DIVIDE_SECOND_BY_FIRST = 5, // Uncertain
}
