using System.Diagnostics.CodeAnalysis;

namespace GameServer.Enums;

[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "TODO")]
public enum ThumperState : byte
{
    NONE = 0,
    AWAITING_CLEARANCE = 1,
    LANDING = 2,
    WARMINGUP = 3,
    THUMPING = 4,
    CLOSING = 5,
    COMPLETED = 6,
    LEAVING = 7,
    DESTROYED = 8,
    RESERVED = 9
}
