using System;
using System.IO;
using System.Numerics;
using AeroMessages.Common;
using AeroMessages.GSS.Character;

namespace GameServer.Entities.Character;

public record WarpaintPattern
{
    public uint Id;
    public Vector4 Transform; // Rotation in degrees, scale, horizontal, vertical
    public byte Usage; // 0, 1, 2, 3 permitted
}

public class WarpaintPatternData
{
    // Scale and the two offsets go on the wire as 16 bit fixed point with twelve fractional bits, not as IEEE
    // halfs: a scale of 1.0 is 4096, 0.25 is 1024 and 4 is 16384. Aero types the field as HalfVector4 because
    // that is a bag of four ushorts, but the bits in them are not halfs.
    public const float WireFixedPoint = 4096f;

    // One fixed point value tops out just under 16. Clamp rather than cast.
    public const float WireMaxValue = ushort.MaxValue / WireFixedPoint;

    // Rotation is not on that scale, and it is not an IEEE half either: the client holds an angle as degrees times 90
    public const float WireAngleUnitsPerDegree = 90f;

    // One turn in the same units, which is also where the angle wraps.
    public const float WireAngleFullTurn = 360f * WireAngleUnitsPerDegree;

    public WarpaintPattern[] Entries;

    /// <summary>
    ///     Encode scale or an offset as the fixed point number the visuals block carries.
    /// </summary>
    /// <param name="value">The value as a real number</param>
    /// <returns>The value times WireFixedPoint</returns>
    public static ushort ToWireValue(float value)
    {
        if (float.IsNaN(value) || value <= 0f)
        {
            return 0;
        }

        // Round rather than truncate.
        return (ushort)Math.Min(Math.Round(value * WireFixedPoint), ushort.MaxValue);
    }

    /// <summary>
    ///     Encode a rotation as the angle the visuals block carries.
    /// </summary>
    /// <param name="degrees">The rotation in degrees, which may be negative or past one turn</param>
    /// <returns>The degrees times WireAngleUnitsPerDegree, wrapped into one turn</returns>
    public static ushort ToWireAngle(float degrees)
    {
        if (float.IsNaN(degrees))
        {
            return 0;
        }

        var units = (long)Math.Round(degrees * WireAngleUnitsPerDegree);
        units %= (long)WireAngleFullTurn;
        if (units < 0)
        {
            units += (long)WireAngleFullTurn;
        }

        return (ushort)units;
    }

    public static WarpaintPatternData Parse(byte[] data)
    {
        var ok = TryParse(data, out WarpaintPatternData result);
        if (ok)
        {
            return result;
        }
        else
        {
            return new WarpaintPatternData();
        }
    }

    public static bool TryParse(byte[] data, out WarpaintPatternData result)
    {
        result = new WarpaintPatternData();
        try
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            var count = reader.ReadUInt32();
            result.Entries = new WarpaintPattern[count];

            for (int i = 0; i < count; i++)
            {
                result.Entries[i] = new()
                {
                    Id = reader.ReadUInt32(),
                    Transform = new(
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle()),
                    Usage = reader.ReadByte(),
                };
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public VisualsPatternBlock[] ToAero()
    {
        if (Entries != null && Entries.Length != 0)
        {
            var blocks = new VisualsPatternBlock[Entries.Length];
            for (int i = 0; i < Entries.Length; i++)
            {
                blocks[i].PatternId = Entries[i].Id;
                blocks[i].TransformValues = new HalfVector4
                {
                    X = new HalfFloat { Value = ToWireAngle(Entries[i].Transform.X) },
                    Y = new HalfFloat { Value = ToWireValue(Entries[i].Transform.Y) },
                    Z = new HalfFloat { Value = ToWireValue(Entries[i].Transform.Z) },
                    W = new HalfFloat { Value = ToWireValue(Entries[i].Transform.W) },
                };
                blocks[i].Usage = Entries[i].Usage;
            }

            return blocks;
        }
        else
        {
            return [];
        }
    }
}