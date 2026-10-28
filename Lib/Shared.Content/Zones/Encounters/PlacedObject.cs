using System.Numerics;

namespace Shared.Content.Zones.Encounters;

/// <summary>A deployable that belongs to another entry, e.g. a repulsor's terminal.</summary>
public sealed record PlacedObject
{
    /// <summary>The SDB deployable id.</summary>
    public required uint Type { get; init; }

    public required Vector3 Position { get; init; }

    public Quaternion Orientation { get; init; } = Quaternion.Identity;
}
