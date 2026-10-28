using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Shared.Content.Spawns;

/// <summary>The area of a spawn region, the <c>type</c> field in the files says which shape.</summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CircleShape), "circle")]
[JsonDerivedType(typeof(PolygonShape), "polygon")]
public abstract record RegionShape
{
    /// <summary>Where distances to the region are measured from, e.g. for activating it.</summary>
    [JsonIgnore]
    public abstract Vector3 Middle { get; }

    /// <summary>How far the shape reaches from <see cref="Middle"/> at most.</summary>
    [JsonIgnore]
    public abstract float BoundingRadius { get; }
}

/// <summary>Groups spawn anywhere on the navmesh within the radius, a radius of 0 spawns them at the center.</summary>
public sealed record CircleShape : RegionShape
{
    public required Vector3 Center { get; init; }

    public float Radius { get; init; }

    [JsonIgnore]
    public override Vector3 Middle => Center;

    [JsonIgnore]
    public override float BoundingRadius => Radius;
}

/// <summary>
///     An area drawn on the terrain, only x and y of the points count for what's inside. The height of a spawn comes from
///     the navmesh or the ground, the points' heights are just where the search for it starts.
/// </summary>
public sealed record PolygonShape : RegionShape
{
    public required IReadOnlyList<Vector3> Points { get; init; }

    [JsonIgnore]
    public override Vector3 Middle
    {
        get
        {
            if (Points.Count == 0)
            {
                return Vector3.Zero;
            }

            var (min, max) = Bounds();
            return new Vector3((min.X + max.X) / 2, (min.Y + max.Y) / 2, Points.Average(p => p.Z));
        }
    }

    [JsonIgnore]
    public override float BoundingRadius => Points.Count == 0 ? 0 : Points.Max(p => Vector2.Distance(new Vector2(p.X, p.Y), new Vector2(Middle.X, Middle.Y)));

    /// <summary>The corners of the bounding box, x and y only.</summary>
    public (Vector2 Min, Vector2 Max) Bounds()
    {
        var min = new Vector2(float.MaxValue);
        var max = new Vector2(float.MinValue);
        foreach (var point in Points)
        {
            min = Vector2.Min(min, new Vector2(point.X, point.Y));
            max = Vector2.Max(max, new Vector2(point.X, point.Y));
        }

        return (min, max);
    }

    /// <summary>Even-odd, so a polygon that crosses itself has holes where it overlaps.</summary>
    public bool Contains(Vector2 point)
    {
        var inside = false;
        for (int i = 0, j = Points.Count - 1; i < Points.Count; j = i++)
        {
            var a = Points[i];
            var b = Points[j];
            if ((a.Y > point.Y) != (b.Y > point.Y) && point.X < ((b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y)) + a.X)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    /// <summary>In square meters, 0 when the points lie on a line and nothing can be inside.</summary>
    public float Area()
    {
        var sum = 0f;
        for (int i = 0, j = Points.Count - 1; i < Points.Count; j = i++)
        {
            sum += (Points[j].X * Points[i].Y) - (Points[i].X * Points[j].Y);
        }

        return MathF.Abs(sum) / 2;
    }
}
