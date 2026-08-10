using System.Collections.Generic;

namespace GameServer.Enums;

public readonly record struct AmmoFlags(uint Raw)
{
    public enum SimulationMode : byte
    {
        /// <summary>
        /// The client sets the mode to 1 if it is set to 0 when loading ammo, unclear what this distinguishes
        /// </summary>
        Basic = 0,

        // Simulates a projectile following a linear interpolation with no gravity
        Linear = 1,

        // Simulates a projectile affected by gravity
        Parabolic = 2,

        // Simulates a projectile homing towards a predefined target
        Homing = 3
    }

    public enum SpeedInterpolationMode : byte
    {
        None = 0,
        LerpFromCurrent = 1,
        LerpFromAmmo = 2
    }

    public SimulationMode Simulation
    {
        get
        {
            var mode = (byte)(Raw & 0b11u);
            if (mode == 0)
            {
                mode = 1;
            }

            return (SimulationMode)mode;
        }
    }

    public SpeedInterpolationMode SpeedInterpolation =>
        (SpeedInterpolationMode)((Raw & 0b110000000000000u) >> 13);

    public bool Unk2                  => (Raw & (1u << 2)) != 0;
    public bool Unk3                  => (Raw & (1u << 3)) != 0;
    public bool UnkHit4               => (Raw & (1u << 4)) != 0;
    public bool Unk5                  => (Raw & (1u << 5)) != 0;
    public bool UnkBounce6            => (Raw & (1u << 6)) != 0;
    public bool RayCastFlag1          => (Raw & (1u << 7)) != 0;
    public bool Unk8                  => (Raw & (1u << 8)) != 0;
    public bool Unk9                  => (Raw & (1u << 9)) != 0;
    public bool UnkHit10              => (Raw & (1u << 10)) != 0;
    public bool Unk11                 => (Raw & (1u << 11)) != 0;
    public bool UnkBounce12           => (Raw & (1u << 12)) != 0;
    public bool Unk15                 => (Raw & (1u << 15)) != 0;
    public bool RayCastFlag8          => (Raw & (1u << 16)) != 0;
    public bool RayCastFlag512        => (Raw & (1u << 17)) != 0;
    public bool Unk18                 => (Raw & (1u << 18)) != 0;
    public bool Unk19                 => (Raw & (1u << 19)) != 0;
    public bool UnkHoming20           => (Raw & (1u << 20)) != 0;

    public static implicit operator AmmoFlags(uint raw) => new(raw);

    public override string ToString()
    {
        var parts = new List<string>
        {
            Simulation.ToString()
        };

        if (SpeedInterpolation != SpeedInterpolationMode.None)
        {
            parts.Add(SpeedInterpolation.ToString());
        }

        CheckAndAdd(parts, "Unk2", Unk2);
        CheckAndAdd(parts, "Unk3", Unk3);
        CheckAndAdd(parts, "UnkHit4", UnkHit4);
        CheckAndAdd(parts, "Unk5", Unk5);
        CheckAndAdd(parts, "UnkBounce6", UnkBounce6);
        CheckAndAdd(parts, "RayCastFlag1", RayCastFlag1);
        CheckAndAdd(parts, "Unk8", Unk8);
        CheckAndAdd(parts, "Unk9", Unk9);
        CheckAndAdd(parts, "UnkHit10", UnkHit10);
        CheckAndAdd(parts, "Unk11", Unk11);
        CheckAndAdd(parts, "UnkBounce12", UnkBounce12);
        CheckAndAdd(parts, "Unk15", Unk15);
        CheckAndAdd(parts, "RayCastFlag8", RayCastFlag8);
        CheckAndAdd(parts, "RayCastFlag512", RayCastFlag512);
        CheckAndAdd(parts, "Unk18", Unk18);
        CheckAndAdd(parts, "Unk19", Unk19);
        CheckAndAdd(parts, "UnkHoming20", UnkHoming20);

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
