using GameServer.Entities.Deployable;
using GameServer.StaticDB.Records.apt;
using GameServer.StaticDB.Records.customdata;
using GameServer.Systems.Aptitude;
using GameServer.Systems.Aptitude.Commands.Impact;
using GameServer.Tests.Fakes;
using Xunit;

namespace GameServer.Tests;

/// <summary>
///     <c>aptgss::ImpactRemoveEffectCommandDef</c> is only complete for a handful of its rows: the definition of
///     the shared glider pad launch ability (1001664..1001667, for instance) says that effects have to be taken
///     away but not which ones.
///
///     That is not a licence to guess: the same chain applied its own launch effect a few commands earlier, so
///     "remove the effects of this ability" would undo it again and leave the player with no boost, no glider
///     permission and an ability the client keeps re-triggering. The command now reports the missing data once and
///     leaves the effects alone.
/// </summary>
public class ImpactRemoveEffectCommandTests
{
    [Fact]
    public void WithoutAnEffectId_LeavesTheEffectsOfTheAbilityAlone()
    {
        var shard = new FakeShard();
        shard.Abilities = new AbilitySystem(shard);

        var pad = CreateDeployable(shard);
        pad.AddEffect(CreateEffect(8070), new Context(shard, pad));

        var context = new Context(shard, pad);
        context.Targets.Push(pad);

        Assert.True(new ImpactRemoveEffectCommand(new ImpactRemoveEffectCommandDef { Id = 1001664 }).Execute(context));

        Assert.Contains(pad.GetActiveEffects(), state => state?.Effect.Id == 8070);
    }

    [Fact]
    public void WithAnEffectId_RemovesItFromEveryTarget()
    {
        var shard = new FakeShard();
        shard.Abilities = new AbilitySystem(shard);

        var pad = CreateDeployable(shard);
        var other = CreateDeployable(shard);
        pad.AddEffect(CreateEffect(8070), new Context(shard, pad));
        other.AddEffect(CreateEffect(8070), new Context(shard, other));

        var context = new Context(shard, pad);
        context.Targets.Push(pad);
        context.Targets.Push(other);

        Assert.True(new ImpactRemoveEffectCommand(new ImpactRemoveEffectCommandDef { Id = 1, EffectId = 8070 }).Execute(context));

        Assert.DoesNotContain(pad.GetActiveEffects(), state => state?.Effect.Id == 8070);
        Assert.DoesNotContain(other.GetActiveEffects(), state => state?.Effect.Id == 8070);
    }

    [Fact]
    public void RemoveFromSelf_OnlyTouchesTheOwnerOfTheChain()
    {
        var shard = new FakeShard();
        shard.Abilities = new AbilitySystem(shard);

        var pad = CreateDeployable(shard);
        var other = CreateDeployable(shard);
        pad.AddEffect(CreateEffect(8070), new Context(shard, pad));
        other.AddEffect(CreateEffect(8070), new Context(shard, other));

        var context = new Context(shard, pad);
        context.Targets.Push(other);

        Assert.True(new ImpactRemoveEffectCommand(new ImpactRemoveEffectCommandDef { Id = 2, EffectId = 8070, RemoveFromSelf = true }).Execute(context));

        Assert.DoesNotContain(pad.GetActiveEffects(), state => state?.Effect.Id == 8070);
        Assert.Contains(other.GetActiveEffects(), state => state?.Effect.Id == 8070);
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
            },
        };
    }
}
