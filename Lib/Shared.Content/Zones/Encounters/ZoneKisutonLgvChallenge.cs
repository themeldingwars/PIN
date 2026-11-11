using System.Collections.Generic;
using System.Numerics;

namespace Shared.Content.Zones.Encounters;

public sealed record ZoneKisutonLgvChallenge : ZoneEncounter
{
    public ushort RaceType { get; init; }

    public uint ChallengeNameTextId { get; init; }

    public uint TrackNameTextId { get; init; }

    public ushort Laps { get; init; } = 1;

    public uint LeaderboardId { get; init; }

    public required Pose Start { get; init; }

    public required Pose Terminal { get; init; }

    public required Pose LeaderboardTerminal { get; init; }

    public required IReadOnlyList<Vector3> Waypoints { get; init; }
}
