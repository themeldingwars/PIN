using System.Text.Json.Serialization;

namespace Shared.Content.Spawns;

/// <summary>Where a spawn table spawns in a zone. Lives in <c>zones/&lt;zone id&gt;/regions.json</c>.</summary>
public sealed record SpawnRegion : ContentEntry
{
    public required string Id { get; init; }

    /// <summary>The id of the spawn table.</summary>
    public required string Table { get; init; }

    public required RegionShape Shape { get; init; }

    /// <summary>How many groups the region keeps alive.</summary>
    public uint Population { get; init; } = 1;

    /// <summary>Overrides the zone's <see cref="Zones.PopulationSettings.ActivationRadius"/>.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? ActivationRadius { get; init; }

    /// <summary>Overrides the zone's <see cref="Zones.PopulationSettings.DespawnAfterSeconds"/>.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public uint? DespawnAfterSeconds { get; init; }

    [JsonIgnore]
    public override string Key => Id;
}
