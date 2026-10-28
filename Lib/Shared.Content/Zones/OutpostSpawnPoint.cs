using System.Numerics;

namespace Shared.Content.Zones;

public sealed record OutpostSpawnPoint
{
    public required Vector3 Position { get; init; }

    public Quaternion Orientation { get; init; } = Quaternion.Identity;

    public Vector3 AimDirection { get; init; }
}
