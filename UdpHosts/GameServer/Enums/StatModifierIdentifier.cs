using System.Diagnostics.CodeAnalysis;

namespace GameServer.Enums;

[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "TODO")]
public enum StatModifierIdentifier : ushort
{
    // TODO: Maybe rewrite this, not sure if the mapping is supposed to be 1 to 1
    RunSpeedMult = 1,
    FwdRunSpeedMult = 2, // 1189
    JumpHeightMult = 3, // 1189
    DamageTaken = 4, // Damage Resistance? Boomerang, Heavy Armor
    DamageDealt = 5, // 1189
    Health = 6, // 1189
    MaxHealth = 7, // 1189
    Shields = 8, // 1189
    MaxShields = 9, // 1189
    AirControlMult = 10, // Air Control? Based on Hover Mode, command 859100 is loading Hover Mode Air Control stat right before 859099 which modifies this stat
    Energy = 11,
    MaxEnergy = 12,
    EnergyRechargeDelay = 13,
    EnergyRechargeRate = 14,
    ThrustStrengthMult = 15,
    ThrustAirControl = 16, // Air Control? Based Engineer Overclock
    Friction = 17,
    SinBonus = 18,
    SinVulnerability = 19,
    MaxTurnRate = 20, // Seems confirmed by msg 551119, command 1120999, effect 11686
    FireRateModifier = 21,
    AmmoConsumption = 22, // Based on Overcharge command 1626508, seems to hold up
    AccuracyModifier = 23,
    CooldownModifier = 24,
    Progress = 25,

    TurnSpeed = 27,

    // TODO: Map out these
    Unknown_26 = 26,
    Unknown_28 = 28,
    Unknown_29 = 29,
    Unknown_30 = 30,
    Unknown_31 = 31,
    Unknown_38 = 38,
    Unknown_39 = 39,
    Unknown_47 = 47,
    
    GravityMult = 50,
    Unknown_56 = 56, // Jet Energy Consumption? Damage Resistance? Heavy Armor // AirResMult?
    Unknown_57 = 57,
    WeaponChargeupMod = 9914,
    WeaponDamageDealtMod = 9915,
    AirResistanceMult = 9913,
    TimeDilation = 9910,
}