using System.Threading;
using GameServer.Entities;
using GameServer.Entities.Deployable;
using GameServer.StaticDB.Records.apt;
using GameServer.Systems.Aptitude;
using GameServer.Tests.Fakes;
using Xunit;

namespace GameServer.Tests;

/// <summary>
///     Regression tests for the crash where removing one of an entity's expired
///     effects destroyed the entity itself (a RemoveEffect chain ending in
///     DestroyAbilityObjectCommand), and the ability system then went on to
///     process the entity's remaining effects, crashing in
///     <c>EntityManager.FlushChanges</c> with a <c>KeyNotFoundException</c>
///     because the removed entity was no longer in the scoped-players registry.
/// </summary>
public class AbilitySystemTests
{
    [Fact]
    public void Tick_EntityDestroyedByEffectRemoval_DoesNotCrashAndKeepsProcessingOtherEntities()
    {
        var shard = new FakeShard();
        shard.Abilities = new AbilitySystem(shard);

        var victim = CreateDeployable(shard);
        var bystander = CreateDeployable(shard);

        // The victim carries two expired effects. Removing the first one runs
        // its remove chain, which destroys the entity itself; the second
        // effect is still listed in the snapshot ProcessTarget took.
        victim.AddEffect(CreateEffect(9001, removeChain: new Chain
        {
            Id = 9101,
            Commands = [new DestroySelfCommand()]
        }),
        new Context(shard, victim));
        victim.AddEffect(CreateEffect(9002), new Context(shard, victim));

        // A different entity's expired effect must still be processed.
        bystander.AddEffect(CreateEffect(9003), new Context(shard, bystander));

        shard.Abilities.Tick(50, 100_000, CancellationToken.None);

        Assert.False(shard.Entities.ContainsKey(victim.EntityId));
        Assert.True(shard.Entities.ContainsKey(bystander.EntityId));
        Assert.DoesNotContain(e => e is not null, bystander.GetActiveEffects());
    }

    [Fact]
    public void FlushChanges_EntityRemovedBeforeFlush_DoesNotThrow()
    {
        var shard = new FakeShard();
        var entity = CreateDeployable(shard);

        // Create pending view changes, then remove the entity before the
        // changes are flushed again.
        entity.AddEffect(CreateEffect(9001), new Context(shard, entity));
        shard.EntityMan.Remove(entity);
        entity.ClearStatusEffect(0, 42, 9001);

        // Flushing changes of an entity that is no longer in the shard is a no-op.
        shard.EntityMan.FlushChanges(entity);
    }

    private static DeployableEntity CreateDeployable(FakeShard shard)
    {
        var entity = new DeployableEntity(shard, shard.GetNextGuid(), type: 395, abilitySrcId: 0);
        shard.EntityMan.Add(entity.EntityId, entity);
        return entity;
    }

    private static Effect CreateEffect(uint id, Chain removeChain = null)
    {
        return new Effect
        {
            Data = new StatusEffectData
            {
                Id = id,
                UpdateFrequency = 100,
                MaxStackCount = 1
            },
            // A duration chain that always fails marks the effect as expired.
            DurationChain = new Chain
            {
                Id = id + 1000,
                Commands = [new ExpiredCommand()]
            },
            RemoveChain = removeChain
        };
    }

    private sealed class ExpiredCommand : ICommand
    {
        public uint Id { get; set; } = 9100;

        public bool Execute(Context context) => false;
    }

    private sealed class DestroySelfCommand : ICommand
    {
        public uint Id { get; set; } = 9102;

        public bool Execute(Context context)
        {
            // Mirrors DestroyAbilityObjectCommand: the ability object destroys itself.
            context.Shard.EntityMan.Remove((IEntity)context.Self);
            return true;
        }
    }
}
