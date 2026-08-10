using System;
using System.Numerics;
using GameServer.StaticDB.Records.dbitems;

namespace GameServer.Systems.ProjectileSim;

/// <summary>
/// Replicates the client's "drunk missile" projectile perturbation.
/// A deterministic per-frame sinusoidal offset, perpendicular to the projectile velocity, is added
/// to the simulated position. The seed is the projectile trace id.
/// </summary>
public static class DrunkMissile
{
    private const float _minDrunkAmplitude = 0.001f;
    private const float _pi = 3.1415927f;

    /// <summary>
    /// True when the ammo has an active drunk missile (amplitude above the client's 0.001 gate).
    /// </summary>
    /// <param name="ammo">The Ammo to check.</param>
    /// <returns>True if the Ammo properties produces drunk missile</returns>
    public static bool IsActive(Ammo ammo)
    {
        return ammo.DrunkMissileAmplitude > _minDrunkAmplitude;
    }

    /// <summary>
    /// Compute the drunk missile position offset for a given simulation frame.
    /// </summary>
    /// <param name="velocity">Projectile velocity (normalized internally when its magnitude is above 1, matching the client).</param>
    /// <param name="ammo">The ammo record (provides amplitude/frequency/decay).</param>
    /// <param name="dt">Seconds since the projectile was fired.</param>
    /// <param name="t">Normalized flight progress [0,1] (used for decay).</param>
    /// <param name="seed">The projectile seed (PRNG.Trace(time, round)).</param>
    /// <returns>Drunk offset vector</returns>
    public static Vector3 ComputeOffset(Vector3 velocity, Ammo ammo, float dt, float t, uint seed)
    {
        float amplitude = ammo.DrunkMissileAmplitude;
        if (amplitude <= _minDrunkAmplitude)
        {
            return Vector3.Zero;
        }

        float frequency = ammo.DrunkMissileFrequency;
        float decay = ammo.DrunkMissileDecayRangefrac;

        // Decay envelope: max(0, 1 - t/decay) * amplitude when decay is meaningful, else flat amplitude.
        float scale = decay > _minDrunkAmplitude
            ? Math.Max(0f, (1f - (t / decay)) * amplitude)
            : amplitude;

        // Six pseudo-random values derived from the seed. comp1 is driven by seed+0/2/4, comp2 by seed+1/3/5.
        float comp1Amp = PRNG.PRNG.Float(seed + 0) * scale;
        float comp2Amp = PRNG.PRNG.Float(seed + 1) * scale;
        float comp1Freq = PRNG.PRNG.Float(seed + 2) * frequency;
        float comp2Freq = PRNG.PRNG.Float(seed + 3) * frequency;
        float comp1Phase = (PRNG.PRNG.Float(seed + 4) * 2.0f) * _pi;
        float comp2Phase = (PRNG.PRNG.Float(seed + 5) * 2.0f) * _pi;

        // Both terms are zero at dt == 0, so the projectile starts exactly on the base trajectory.
        float comp1 = comp1Amp * (MathF.Sin((comp1Freq * dt) + comp1Phase) - MathF.Sin(comp1Phase));
        float comp2 = comp2Amp * (MathF.Sin((comp2Freq * dt) + comp2Phase) - MathF.Sin(comp2Phase));

        // Build an orthonormal frame (axis = 1) around the velocity direction.
        Vector3 vel = velocity;
        float mag = vel.Length();
        if (mag > 0f && mag != 1f)
        {
            vel /= mag;
        }

        Vector3 perp1;
        if (Math.Abs(vel.Y) <= Math.Abs(vel.X))
        {
            perp1 = Vector3.Normalize(new Vector3(-vel.Z, 0f, vel.X));
        }
        else
        {
            perp1 = Vector3.Normalize(new Vector3(0f, -vel.Z, vel.Y));
        }

        Vector3 perp2 = Vector3.Cross(vel, perp1);

        return (perp2 * comp1) + (perp1 * comp2);
    }
}
