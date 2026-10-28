using System.Text.Json.Serialization;

namespace Shared.Content.Zones.Encounters;

/// <summary>
///     Something that runs in the zone, a race, a repulsor and so on. One kind for all of them, since the game has one
///     encounter entity and tells them apart by type, the <c>type</c> field in the files.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ZoneLgvRace), "lgv-race")]
[JsonDerivedType(typeof(ZoneMeldingRepulsor), "melding-repulsor")]
public abstract record ZoneEncounter : ContentEntry
{
    public required string Id { get; init; }

    [JsonIgnore]
    public override string Key => Id;
}
