using Shared.Content.Spawns;

namespace GameServer.Systems.Ai;

/// <summary>How a spawned group behaves: the defaults of its style, with what the spawn table overrides.</summary>
public sealed record GroupRules
{
    private static readonly GroupRules _herd = new() { Style = GroupStyle.Herd, Spread = 20f };
    private static readonly GroupRules _solo = new() { Style = GroupStyle.Solo, Spread = 20f };

    public required GroupStyle Style { get; init; }

    /// <summary>How far the members spawn and wander from their home, in meters.</summary>
    public required float Spread { get; init; }

    /// <summary>A herd's members share the group's home, a solo member's home is where it spawned.</summary>
    public bool SharesHome => Style == GroupStyle.Herd;

    public static GroupRules For(SpawnGroup group)
    {
        var defaults = group.Style == GroupStyle.Solo ? _solo : _herd;
        return defaults with { Spread = group.Spread ?? defaults.Spread };
    }
}
