using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Shared.Content.Spawns;

/// <summary>NPCs that spawn together, e.g. a herd, and count as dead once all of them are.</summary>
public sealed record SpawnGroup
{
    public uint Weight { get; init; } = 1;

    /// <summary>A herd when left out.</summary>
    public GroupStyle Style { get; init; } = GroupStyle.Herd;

    /// <summary>
    ///     How far the members spread around their home in meters, where they spawn and wander. Overrides the style's
    ///     default.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? Spread { get; init; }

    public required IReadOnlyList<SpawnMember> Members { get; init; }
}
