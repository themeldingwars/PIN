namespace Shared.Content.Zones;

/// <summary>
///     What applies to the whole zone, from <c>zones/&lt;zone id&gt;/zone.json</c>. Without the file, or for what it
///     leaves out, the defaults here.
/// </summary>
public sealed record ZoneSettings
{
    public PopulationSettings Population { get; init; } = new();
}

/// <summary>How the zone's spawn regions behave, a region can override each of these.</summary>
public sealed record PopulationSettings
{
    /// <summary>
    ///     A region fills while a player is within this many meters of its edge. About the client's scope distance, so the
    ///     NPCs are there before they come into view.
    /// </summary>
    public float ActivationRadius { get; init; } = 300f;

    /// <summary>
    ///     How long a region keeps its NPCs after the last player left its activation radius. Long enough that a player
    ///     who turns around doesn't find the area empty.
    /// </summary>
    public uint DespawnAfterSeconds { get; init; } = 180;
}
