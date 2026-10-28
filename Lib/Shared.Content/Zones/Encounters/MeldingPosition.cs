using System.Numerics;

namespace Shared.Content.Zones.Encounters;

public sealed record MeldingPosition
{
    public required uint ControlPointIndex { get; init; }

    public required Vector3 Position { get; init; }
}
