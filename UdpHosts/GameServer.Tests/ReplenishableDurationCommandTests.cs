using GameServer.Entities.Deployable;
using GameServer.StaticDB.Records.customdata;
using GameServer.Systems.Aptitude;
using GameServer.Systems.Aptitude.Commands.Duration;
using GameServer.Tests.Fakes;
using Xunit;

namespace GameServer.Tests;

/// <summary>
///     Regression tests for the boomerang: its effect uses a ReplenishableDuration chain. While that command was
///     a placeholder returning true the duration chain never failed, so the effect was never removed and the
///     ability system kept re-evaluating (and re-triggering the sound of) the boomerang forever.
/// </summary>
public class ReplenishableDurationCommandTests
{
    [Fact]
    public void Execute_ExpiresOnceTheRegisterDurationHasPassed()
    {
        var shard = new FakeShard { CurrentTimeLong = 10_000 };
        var command = new ReplenishableDurationCommand(new ReplenishableDurationCommandDef { Id = 1579559 });

        // Boomerang Duration is authored in seconds.
        var context = new Context(shard, CreateDeployable(shard)) { Register = 2f };

        Assert.True(command.Execute(context));

        shard.CurrentTimeLong = 11_000;
        Assert.True(command.Execute(context));

        shard.CurrentTimeLong = 12_500;
        Assert.False(command.Execute(context));
    }

    [Fact]
    public void Execute_WithoutADurationInTheRegister_StillExpires()
    {
        var shard = new FakeShard { CurrentTimeLong = 0 };
        var command = new ReplenishableDurationCommand(new ReplenishableDurationCommandDef { Id = 1 });
        var context = new Context(shard, CreateDeployable(shard)) { Register = 0f };

        Assert.True(command.Execute(context));

        shard.CurrentTimeLong = 120_000;
        Assert.False(command.Execute(context));
    }

    [Fact]
    public void Execute_TracksEachContextSeparately()
    {
        var shard = new FakeShard { CurrentTimeLong = 0 };
        var command = new ReplenishableDurationCommand(new ReplenishableDurationCommandDef { Id = 2 });

        var first = new Context(shard, CreateDeployable(shard)) { Register = 2f };
        Assert.True(command.Execute(first));

        shard.CurrentTimeLong = 5_000;

        // A second application starts its own duration instead of inheriting the expired one.
        var second = new Context(shard, CreateDeployable(shard)) { Register = 2f };
        Assert.False(command.Execute(first));
        Assert.True(command.Execute(second));
    }

    private static DeployableEntity CreateDeployable(FakeShard shard)
    {
        var entity = new DeployableEntity(shard, shard.GetNextGuid(), type: 395, abilitySrcId: 0);
        shard.EntityMan.Add(entity.EntityId, entity);
        return entity;
    }
}
