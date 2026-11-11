namespace GameServer.Systems.Duels;

public interface IDuelRules
{
    uint CountdownMs { get; }

    // The client declines on its own after 120 seconds, this only cleans up after one that never answered
    uint ProposalTimeoutMs { get; }

    uint TimeLimitMs { get; }

    uint ReviveDelayMs { get; }

    // 0 plants no flag
    uint FlagDeployableId { get; }

    float FlagDistance { get; }
}
