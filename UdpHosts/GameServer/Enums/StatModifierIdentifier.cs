using System.Diagnostics.CodeAnalysis;

namespace GameServer.Enums;

[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "TODO")]
public enum StatModifierIdentifier : ushort
{
    // TODO: Maybe rewrite this, not sure if the mapping is supposed to be 1 to 1
    RunSpeedMult = 1,
    FireRateModifier = 21,
    AmmoConsumption = 22, // Based on Overcharge command 1626508, seems to hold up

    // Related
    MaxTurnRate = 20, // Seems confirmed by msg 551119, command 1120999, effect 11686
    GravityMult = 50, // Either GravityMult or TurnSpeed
    TurnSpeed = 27, // Either GravityMult or TurnSpeed

    Unknown_10 = 16, // Air Control? Based Engineer Overclock
    AirControlMult = 10, // Air Control? Based on Hover Mode, command 859100 is loading Hover Mode Air Control stat right before 859099 which modifies this stat

    Unknown_4 = 4, // Damage Resistance? Boomerang, Heavy Armor
    Unknown_56 = 56, // Jet Energy Consumption? Damage Resistance? Heavy Armor

    // TODO: Map out these
    Unknown_2 = 2,
    Unknown_3 = 3,
    Unknown_5 = 5,
    Unknown_6 = 6,
    Unknown_7 = 7,
    Unknown_8 = 8,
    Unknown_9 = 9,
    Unknown_11 = 11,
    Unknown_12 = 12,
    Unknown_13 = 13,
    Unknown_14 = 14,
    Unknown_15 = 15,
    Unknown_17 = 17,
    Unknown_18 = 18,
    Unknown_19 = 19,
    Unknown_23 = 23,
    Unknown_24 = 24,
    Unknown_25 = 25,
    Unknown_26 = 26,
    Unknown_28 = 28,
    Unknown_29 = 29,
    Unknown_30 = 30,
    Unknown_31 = 31,
    Unknown_38 = 38,
    Unknown_39 = 39,
    Unknown_47 = 47,
    Unknown_57 = 57,
    FwdRunSpeedMult = 9901,
    JumpHeightMult = 9902,
    ThrustStrengthMult = 9904,
    ThrustAirControl = 9905,
    AirResistanceMult = 9913,
    Friction = 9906,
    TimeDilation = 9910,
    AccuracyModifier = 9911,
    WeaponChargeupMod = 9914,
    WeaponDamageDealtMod = 9915,
}