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

public static class ThumperStateExtension
{
    public static uint CountdownTime(this ThumperState state)
    {
        return state switch
               {
                   ThumperState.WARMINGUP => 10_000,
                   ThumperState.THUMPING => 300_000,
                   ThumperState.CLOSING => 6_000,
                   ThumperState.COMPLETED => 120_000,
                   ThumperState.LEAVING => 12_000,
                   _ => 0,
               };
    }
}