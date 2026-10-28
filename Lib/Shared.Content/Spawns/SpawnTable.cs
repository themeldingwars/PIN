using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Shared.Content.Spawns;

/// <summary>What spawns, reusable across zones and regions. Lives in <c>spawn-tables/&lt;family&gt;.json</c>.</summary>
public sealed record SpawnTable : ContentEntry
{
    public required string Id { get; init; }

    /// <summary>The level each NPC gets, rolled per NPC. Without one they spawn at the monster's default level.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public NumberRange? Level { get; init; }

    /// <summary>How long a killed group stays dead before the region spawns a new one.</summary>
    public required NumberRange RespawnSeconds { get; init; }

    /// <summary>A spawn picks one of these by weight.</summary>
    public required IReadOnlyList<SpawnGroup> Groups { get; init; }

    [JsonIgnore]
    public override string Key => Id;
}
