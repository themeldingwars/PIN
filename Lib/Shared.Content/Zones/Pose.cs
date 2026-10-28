using System.Numerics;

namespace Shared.Content.Zones;

public sealed record Pose
{
    public required Vector3 Position { get; init; }

    public Quaternion Orientation { get; init; } = Quaternion.Identity;
}
