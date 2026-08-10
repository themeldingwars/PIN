using System.Collections.Generic;

namespace GameServer.Enums;

public readonly record struct WeaponFlags(uint Raw)
{
    // Treated as a value in MaybeApplyWeaponTweak
    public enum BurstMode : byte
    {
        M0 = 0, // Standard?
        M1 = 1, // HMG?
        M2 = 2, // Bolt Action?
        M3 = 3,
        M4 = 4, // Fire starts chargeup
        M5 = 5, // gets double recoil alt fire?
        M6 = 6, // Rated limited
        M7 = 7 // Similar to M1
    }

    /// <summary>
    /// A value that alters behavior in various sim funcs
    /// </summary>
    public BurstMode Mode =>
        (BurstMode)((Raw & 7u) >> 0);

    /// <summary>
    /// 81% of all weapon templates have this flag. Seems like if this is set on either mode, both modes use the same clip.
    /// </summary>
    public bool LinkedModes => (Raw & (1u << 3)) != 0;

    /// <summary>
    /// I think this is related to having to click for every shot, need to compare with BurstMode.M2
    /// </summary>
    public bool BoltAction => (Raw & (1u << 4)) != 0; // I think this is related to having to click for every shot

    /// <summary>
    /// Set on some primaries, couple of cases where its set on the alt-fire but not the main-fire "alt-fire availability". "Alternate fire can be selected while primary is firing / not in chargeup state"
    /// </summary>
    public bool AltFireAvailable => (Raw & (1u << 5)) != 0;

    /// <summary>
    /// ???
    /// </summary>
    public bool Unk6 => (Raw & (1u << 6)) != 0;

    /// <summary>
    /// Bio Rifle has this.
    /// </summary>
    public bool Unk7ExtraParam => (Raw & (1u << 7)) != 0;

    /// <summary>
    /// Bio Rifle has this
    /// </summary>
    public bool Unk8TargetList => (Raw & (1u << 8)) != 0;

    /// <summary>
    /// "per burst sustained fire branch"
    /// </summary>
    public bool Beam => (Raw & (1u << 9)) != 0;

    /// <summary>
    /// "trigers a beam reticle efect during main fire handler"
    /// </summary>
    public bool LaserMG => (Raw & (1u << 10)) != 0;

    /// <summary>
    /// PlasmaCannon?
    /// </summary>
    public bool PlasmaCannon => (Raw & (1u << 11)) != 0;

    /// <summary>
    /// "gates the chargeup spin-wait at fire time"
    /// </summary>
    public bool MaybeChargeupHold => (Raw & (1u << 12)) != 0;

    /// <summary>
    /// FUN_007bda90, looks like it has some interaction with weapons that shoot multiple rounds per burst ("ammo rate-limited continious fire).
    /// </summary>
    public bool Unk13 => (Raw & (1u << 13)) != 0;

    /// <summary>
    /// ???
    /// </summary>
    public bool Unk14 => (Raw & (1u << 14)) != 0;

    /// <summary>
    /// Changes something in FireWeapon "special alt mode"
    /// </summary>
    public bool HealBeam => (Raw & (1u << 15)) != 0;

    /// <summary>
    /// Fusion Cannon and Railgun/Tesla
    /// </summary>
    public bool SpecialStop => (Raw & (1u << 16)) != 0;

    public static implicit operator WeaponFlags(uint raw) => new(raw);

    public override string ToString()
    {
        var parts = new List<string>();
        parts.Add(Mode.ToString());

        CheckAndAdd(parts, "LinkedModes", LinkedModes);
        CheckAndAdd(parts, "BoltAction", BoltAction);
        CheckAndAdd(parts, "AltFireAvailable", AltFireAvailable);
        CheckAndAdd(parts, "Unk6", Unk6);
        CheckAndAdd(parts, "Unk7ExtraParam", Unk7ExtraParam);
        CheckAndAdd(parts, "Unk8TargetList", Unk8TargetList);
        CheckAndAdd(parts, "Beam", Beam);
        CheckAndAdd(parts, "LaserMG", LaserMG);
        CheckAndAdd(parts, "PlasmaCannon", PlasmaCannon);
        CheckAndAdd(parts, "MaybeChargeupHold", MaybeChargeupHold);
        CheckAndAdd(parts, "Unk13", Unk13);
        CheckAndAdd(parts, "Unk14", Unk14);
        CheckAndAdd(parts, "HealBeam", HealBeam);
        CheckAndAdd(parts, "SpecialStop", SpecialStop);

        return $"0x{Raw:X5} [{string.Join(", ", parts)}]";
    }

    private static void CheckAndAdd(List<string> parts, string name, bool value)
    {
        if (value)
        {
            parts.Add(name);
        }
    }
}