using System.Collections.Generic;
using System.Numerics;

namespace Shared.Content.Zones.Encounters;

public sealed record ZoneKisutonLgvChallenge : ZoneEncounter
{
    public ushort RaceType { get; init; }

    /// <summary>The challenge's name, an SDB text id.</summary>
    public uint ChallengeNameTextId { get; init; }

    /// <summary>The track's name, an SDB text id.</summary>
    public uint TrackNameTextId { get; init; }

    public ushort Laps { get; init; } = 1;

    public uint LeaderboardId { get; init; }

    public required Pose Start { get; init; }

    public required Pose Terminal { get; init; }

    public required Pose LeaderboardTerminal { get; init; }

    /// <summary>The checkpoints in the order they're driven, a lap goes from the first to the last.</summary>
    public required IReadOnlyList<Vector3> Waypoints { get; init; }
}
