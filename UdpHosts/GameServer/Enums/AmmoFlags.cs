using System;

namespace GameServer.Enums;

[Flags]
public enum AmmoFlags
{
    StraightFlight      = 0x000001, // Alternatively this could be no-gravity
    LobbedFlight        = 0x000002, // Alternatively it just enables arcing? When combined with Straight (Bio Rifle), the visual looks Lobbed but the simulation is Straight
    Common2             = 0x000004, // Might have something to do with additional projectile effects
    Unk3                = 0x000008,
    MaybeDrunk          = 0x000010,
    Unk5                = 0x000020,
    Unk6                = 0x000040,
    Unk7                = 0x000080,
    Common8             = 0x000100, // Very common and combined with Straight/Lobbed/Drunk. Maybe just travel time? Or maybe it's like an opposite to Common2, the visuals?
    MaybeHoming         = 0x000200,
    Unk10               = 0x000400,
    MaybeInstant        = 0x000800,
    MaybeTouchAbility   = 0x001000,
    MaybeAirburst       = 0x020000
}