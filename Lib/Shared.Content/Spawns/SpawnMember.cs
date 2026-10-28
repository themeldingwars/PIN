namespace Shared.Content.Spawns;

public sealed record SpawnMember
{
    /// <summary>The SDB monster id.</summary>
    public required uint Monster { get; init; }

    public NumberRange Count { get; init; } = new(1, 1);
}
