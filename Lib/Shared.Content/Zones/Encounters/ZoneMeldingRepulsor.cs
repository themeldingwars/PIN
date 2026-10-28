namespace Shared.Content.Zones.Encounters;

public sealed record ZoneMeldingRepulsor : ZoneEncounter
{
    public required string PerimeterSet { get; init; }

    public required PlacedObject Repulsor { get; init; }

    public required PlacedObject Terminal { get; init; }

    /// <summary>Where the perimeter moves to once the repulsor is up.</summary>
    public required MeldingPosition MeldingPosition { get; init; }
}
