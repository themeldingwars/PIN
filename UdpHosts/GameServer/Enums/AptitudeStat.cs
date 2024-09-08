using System.Diagnostics.CodeAnalysis;

namespace GameServer.Enums;

[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "TODO")]

// Sourced from 1189 commondb.sdb
public enum AptitudeStat : ushort
{
    Speed = 1,
    ForwardSpeed = 2,
    JumpHeight = 3,
    DamageTaken = 4,
    DamageDealt = 5,
    Health = 6,
    MaxHealth = 7,
    Shields = 8,
    MaxShields = 9,
    AirControl = 10,
    Energy = 11,
    MaxEnergy = 12,
    EnergyRechargeDelay = 13,
    EnergyRechargeRate = 14,
    ThrustStrength = 15,
    ThrustAirControl = 16,
    Friction = 17,
    SinBonus = 18,
    SinVulnerability = 19,
    TurnRate = 20,
    FireRateModifier = 21,
    AmmoConsumption = 22,
    AccuracyModifier = 23,
    CooldownModifier = 24,
    Progress = 25,

    /* Extras from 1962 that we don't know
    Unknown_26 = 26,
    Unknown_28 = 28,
    Unknown_29 = 29,
    Unknown_30 = 30,
    Unknown_31 = 31,
    Unknown_38 = 38,
    Unknown_39 = 39,
    Unknown_47 = 47,
    Unknown_56 = 56, // Heavy Armor
    Unknown_57 = 57,
    */
}