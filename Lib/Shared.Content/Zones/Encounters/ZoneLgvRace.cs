namespace Shared.Content.Zones.Encounters;

public sealed record ZoneLgvRace : ZoneEncounter
{
    public uint LeaderboardId { get; init; }

    /// <summary>The race's name, an SDB text id.</summary>
    public uint LabelTextId { get; init; }

    public uint StartDialog { get; init; }

    public required Pose Start { get; init; }

    public required Pose Finish { get; init; }

    public required Pose Terminal { get; init; }

    public uint TimeLimitMs { get; init; }

    public uint BonusTimeLimitMs { get; init; }
}
