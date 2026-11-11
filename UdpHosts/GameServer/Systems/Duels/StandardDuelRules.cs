namespace GameServer.Systems.Duels;

public sealed class StandardDuelRules : IDuelRules
{
    public uint CountdownMs { get; init; } = 10_000;

    public uint ProposalTimeoutMs { get; init; } = 130_000;

    public uint TimeLimitMs { get; init; } = 300_000;

    public uint ReviveDelayMs { get; init; } = 2_000;

    // _DuelMarker aka duel flag, its spawn ability applies status effect 2389
    public uint FlagDeployableId { get; init; } = 614;

    public float FlagDistance { get; init; } = 2f;
}
