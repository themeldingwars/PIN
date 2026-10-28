using System.Numerics;
using System.Text.Json.Serialization;

namespace Shared.Content.Zones;

public sealed record ZoneDeployable : ContentEntry
{
    public required string Id { get; init; }

    /// <summary>The SDB deployable id.</summary>
    public required uint Type { get; init; }

    public required Vector3 Position { get; init; }

    public Quaternion Orientation { get; init; } = Quaternion.Identity;

    [JsonIgnore]
    public override string Key => Id;
}
