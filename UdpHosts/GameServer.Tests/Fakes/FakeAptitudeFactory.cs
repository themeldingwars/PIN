using System.Collections.Generic;
using GameServer.Systems.Aptitude;

namespace GameServer.Tests.Fakes;

/// <summary>
/// Supplies small effect graphs to the real ability runtime, without loading a client installation or
/// mutating the process-wide SDBInterface dictionaries. Commands in the graphs are the production commands.
/// </summary>
public sealed class FakeAptitudeFactory : Factory
{
    public FakeAptitudeFactory(IShard shard)
        : base(shard)
    {
    }

    public Dictionary<uint, Effect> Effects { get; } = [];
    public Dictionary<uint, Chain> Chains { get; } = [];

    public override Effect LoadEffect(uint effectId) => Effects.GetValueOrDefault(effectId);
    public override Chain LoadChain(uint chainId) => Chains[chainId];
}
