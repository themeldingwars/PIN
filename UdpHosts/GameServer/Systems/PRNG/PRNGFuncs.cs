using System;
using System.Numerics;

namespace GameServer;

/// <summary>
/// Provides PRNG functions used for weapons and projectiles, etc.
/// </summary>
public static partial class PRNG
{
    private static readonly float _slMinSpreadVariance = 0.699999988079f; // 0x3f333333

    public static void TestSpread()
    {
        Vector3 final = new Vector3(-997.872070f, -57.570068f, 39.467823f);
        Vector3 dir = new Vector3(-0.997563f, -0.057552f, 0.039456f);
        Vector3 aim = new Vector3(-0.9987106323242188f, -0.03268186002969742f, 0.03884787857532501f);
        float speed = 1000f;
        uint time = 32404587;
        float spreadPct = 3.025f;
        byte slotIndex = 2;
        byte round = 0;

        Vector3 aimForward = aim;
        Vector3 aimRight = Vector3.Normalize(Vector3.Cross(aimForward, Vector3.UnitZ));
        Vector3 aimUp = Vector3.Normalize(Vector3.Cross(aimRight, aimForward));
        Vector3 lastSpreadDirection = Vector3.Zero;
        uint lastSpreadTime = time;
        PRNG.Spread(time, slotIndex, round, aimForward, aimRight, aimUp, spreadPct, lastSpreadDirection, lastSpreadTime, out Vector3 result);
        result = Vector3.Normalize(result);

        Vector3 finalResult = new Vector3(aim.X, aim.Y, aim.Z) + (result * speed);
        Console.WriteLine($"RES Generated ({result.X}, {result.Y}, {result.Z})");
        Console.WriteLine($"RES Expected ({dir.X}, {dir.Y}, {dir.Z})");

        Console.WriteLine($"FIN Generated ({finalResult.X}, {finalResult.Y}, {finalResult.Z})");
        Console.WriteLine($"FIN Expected ({final.X}, {final.Y}, {final.Z})");

        throw new Exception("Check");
    }

    public static uint Trace(uint time, byte bullet)
    {
        uint mod = time + bullet;
        byte u = (byte)(bullet ^ (uint)(((ulong)bullet * 0x797a8d77) >> 32));
        uint v = mod ^ (uint)(((ulong)mod * 0x797a8d77) >> 32);

        byte a = Table[u];
        byte b = Table[v & 0xFF];
        byte c = Table[v >> 24];
        byte d = Table[v >> 8 & 0xFF];
        byte e = Table[v >> 16 & 0xFF];
        uint t1 = (uint)(0x00c1c1c1 | (a << 24));
        uint t2 = (uint)(b << 24 | c << 16 | d << 8 | e);
        return t1 ^ t2 ^ bullet ^ time;
    }

    public static void Spread(uint time, byte slotIndex, byte bullet, Vector3 aimForward, Vector3 aimRight, Vector3 aimUp, float spreadPct, Vector3 lastSpreadDir, uint lastSpreadTime, out Vector3 result)
    {
        if (spreadPct < 0.001)
        {
            result = aimForward;
            return;
        }

        float adjustedSpreadPct = spreadPct * 0.01f;
        bool ignoreMagnitudeCheck = (((time - lastSpreadTime - 1000u) >> 0x1F) & 1) == 0;
        float spreadVarianceMagnitude = _slMinSpreadVariance * _slMinSpreadVariance * adjustedSpreadPct * adjustedSpreadPct;
        float v1, v2;
        byte retry = 0;
        do
        {
            SpreadInner(time, slotIndex, bullet, retry, out v1, out v2);
            float spreadFactorH = v1 * adjustedSpreadPct;
            float spreadFactorV = v2 * adjustedSpreadPct;
            result.X = (aimRight.X * spreadFactorH) + aimForward.X + (aimUp.X * spreadFactorV);
            result.Y = (aimRight.Y * spreadFactorH) + aimForward.Y + (aimUp.Y * spreadFactorV);
            result.Z = (aimRight.Z * spreadFactorH) + aimForward.Z + (aimUp.Z * spreadFactorV);
            Vector3 test = result - lastSpreadDir;
            float testMagnitude = (test.X * test.X) + (test.Y * test.Y) + (test.Z * test.Z);
            if (ignoreMagnitudeCheck || spreadVarianceMagnitude <= testMagnitude)
            {
                return;
            }

            retry++;   
        }
        while (retry < 4);
    }

    private static void SpreadInner(uint time, byte slotIndex, byte bullet, byte retry, out float v1, out float v2)
    {
        uint modulator = (uint)((((((slotIndex * 0x100) + bullet) * 0x100) + retry) * 0x100) + time);
        byte attempt = 0;
        do
        {
            uint uVar1 = GetUVar(modulator);
            uint uVar2 = GetUVar(modulator - 0x3df);
            int iVar1 = GetIVar(uVar1);
            int iVar2 = GetIVar(uVar2);
            
            v1 = GetV(uVar1, iVar1);
            v2 = GetV(uVar2, iVar2);
            if ((v2 * v2) + (v1 * v1) < 1.0)
            {
                return;
            }

            attempt++;
            modulator++;
        }
        while (attempt < 4);
    }

    private static int GetIVar(uint input)
    {
        uint a = Table[input & 0xFF];
        uint b = Table[input >> 0x18];
        uint c = Table[input >> 8 & 0xFF];
        return (int)((a << 16) | (b << 8) | c);
    }

    private static uint GetUVar(uint input)
    {
        return (uint)(((ulong)input * 0x797a8d77) >> 0x20) ^ input;
    }

    private static float GetV(uint uVar, int iVar)
    {
        uint d = Table[uVar >> 0x10 & 0xFF];
        return (float)(((((uint)iVar << 8) | d) * 2.328306e-10 * 2.0) - 1.0);
    }
}