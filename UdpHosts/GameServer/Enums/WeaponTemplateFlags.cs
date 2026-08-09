using System;

namespace GameServer.Enums;

[Flags]
public enum WeaponTemplateFlags
{
    SustainedFire    = 0x0001, // burst=negative (100%)  Maybe just that spread starts the other way around
    Unk2             = 0x0002,
    Overcharge       = 0x0004, // But what

    Common3          = 0x0008, // 81% of all weapon templates have this flag. Might correlate with aim returning to original firing pos.
    BoltAction       = 0x0010, // I think this is more like Bolt Action

    Common5          = 0x0020, // While set on some primaries it seems often set on ADS as well, maybe its associated with the effect

    Unk6           = 0x0040,
    Unk7           = 0x0080, // Bio Rifle has these, I think one of them is related to the aim shifting rather than completely recovering
    Unk8           = 0x0100, // Bio Rifle has these, I think one of them is related to the aim shifting rather than completely recovering
    Beam           = 0x0200,
    LaserMG        = 0x0400,
    PlasmaCannon   = 0x0800, // Recover ammo / reload forbidden?
    MaybeHoming    = 0x2000,
    HealBeam       = 0x4000,
    RailgunTesla   = 0x8000 // This is also set for the Fusion Cannon though
}