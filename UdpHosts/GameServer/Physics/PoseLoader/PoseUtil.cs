#nullable enable

using System;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace GameServer.Physics.PoseLoader;

public class PoseUtil
{
    public static Vector3 ParseVector3(string input)
    {
        var cleaned = input.Trim('<', '>', ' ');
        var parts = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3)
        {
            throw new FormatException($"Invalid Vector3 format: {input}");
        }

        return new Vector3(
            float.Parse(parts[0], CultureInfo.InvariantCulture),
            float.Parse(parts[1], CultureInfo.InvariantCulture),
            float.Parse(parts[2], CultureInfo.InvariantCulture));
    }

    public static Quaternion ParseRotation(string input)
    {
        var vectors = input.Split(['>'], StringSplitOptions.RemoveEmptyEntries)
                           .Select(v => ParseVector3(v + ">")) // Add back '>' so it parses correctly
                           .ToArray();

        if (vectors.Length != 3)
        {
            throw new FormatException($"Invalid Matrix3x3 format: {input}");
        }

        var matrix = new Matrix4x4(
            vectors[0][0],
            vectors[0][1],
            vectors[0][2],
            0,
            vectors[1][0],
            vectors[1][1],
            vectors[1][2],
            0,
            vectors[2][0],
            vectors[2][1],
            vectors[2][2],
            0,
            0,
            0,
            0,
            0);
        return Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(matrix));
    }

    public static float? TryParseFloat(string? input)
    {
        if (float.TryParse(input?.Trim('"'), NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return null;
    }

    public static int? TryParseInt(string? input)
    {
        if (int.TryParse(input?.Trim('"'), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return null;
    }
}