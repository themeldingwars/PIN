using System.Collections.Generic;
using GameServer.Entities.Deployable;
using GameServer.StaticDB.Records.apt;
using GameServer.Systems.Aptitude;
using GameServer.Tests.Fakes;
using Xunit;

namespace GameServer.Tests;

/// <summary>
///     Regression tests for the glider pad: while a player stands on a deployed pad the client keeps sending
///     LocalProximityAbilitySuccess several times a second. Re-running the whole chain each time re-applied and
///     re-flushed the same status effects (and spawned another projectile), which stalled the connection of
///     everyone in range. The activation must be skipped while the effects it applied are still active.
/// </summary>
public class ProximityAbilityRetriggerTests
{
    [Fact]
    public void AppliedEffectRecord_TracksWhetherTheEffectIsStillOnTheTarget()
    {
        var shard = new FakeShard();
        shard.Abilities = new AbilitySystem(shard);

        var pad = CreateDeployable(shard);
        var context = new Context(shard, pad)
        {
            AppliedEffects = []
        };

        Assert.True(shard.Abilities.DoApplyEffect(0, pad, context));
        Assert.Empty(context.AppliedEffects);

        var effect = CreateEffect(4242);
        var state = pad.AddEffect(effect, context);
        var record = new AppliedEffectRecord(pad, state);

        Assert.True(record.IsStillActive());

        pad.ClearEffect(state);

        Assert.False(record.IsStillActive());
    }

    [Fact]
    public void CopyContext_KeepsTheAppliedEffectsList()
    {
        var shard = new FakeShard();
        var pad = CreateDeployable(shard);
        List<AppliedEffectRecord> applied = [];
        var context = new Context(shard, pad) { AppliedEffects = applied };

        var copy = Context.CopyContext(context);

        Assert.Same(applied, copy.AppliedEffects);
    }

    private static DeployableEntity CreateDeployable(FakeShard shard)
    {
        var entity = new DeployableEntity(shard, shard.GetNextGuid(), type: 395, abilitySrcId: 0);
        shard.EntityMan.Add(entity.EntityId, entity);
        return entity;
    }

    private static Effect CreateEffect(uint id)
    {
        return new Effect
        {
            Data = new StatusEffectData
            {
                Id = id,
                UpdateFrequency = 100,
                MaxStackCount = 1
            }
        };
    }
}
