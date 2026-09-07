namespace GameServer.Systems.Ai;

/// <summary>
///     The out of the box AI tuning. Every value can be replaced by passing a
///     custom <see cref="IAiRules" /> to <see cref="AiEngine" />.
/// </summary>
public class StandardAiRules : IAiRules
{
    public bool Enabled { get; init; } = true;

    public float AggroRadius { get; init; } = 55f;

    public float AttackRange { get; init; } = 45f;

    public float AttackRangeExit { get; init; } = 52f;

    public float StandoffRange { get; init; } = 4f;

    public float LeashRadius { get; init; } = 120f;

    public float HomeArrivalRadius { get; init; } = 2f;

    public int AttackCooldownMs { get; init; } = 1200;

    public int AttackDamage { get; init; } = 180;

    public int TargetLostTimeoutMs { get; init; } = 6000;

    public int PerceptionIntervalMs { get; init; } = 200;

    public int MovementIntervalMs { get; init; } = 50;

    public bool SnapToGround { get; init; } = true;

    public float GroundOffset { get; init; }

    public float DefaultMoveSpeed { get; init; } = 5f;

    public float DefaultChaseSpeed { get; init; } = 8.5f;

    public float MinTrustedSpeed { get; init; } = 0.25f;

    public float MaxTrustedSpeed { get; init; } = 35f;
}
