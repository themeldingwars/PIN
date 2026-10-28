using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Shared.Content.Zones;

public sealed record ZoneMelding : ContentEntry
{
    public required string Id { get; init; }

    /// <summary>The name of the melding perimeter set in the zone's data.</summary>
    public required string PerimeterSet { get; init; }

    public uint TimestampMicro { get; init; }

    public uint DurationMicro { get; init; }

    public byte Unk3 { get; init; }

    /// <summary>The perimeter's control points, each with the tangent at the same index in <see cref="Offsets"/>.</summary>
    public required IReadOnlyList<Vector3> ControlPoints { get; init; }

    public required IReadOnlyList<Vector3> Offsets { get; init; }

    [JsonIgnore]
    public override string Key => Id;
}
