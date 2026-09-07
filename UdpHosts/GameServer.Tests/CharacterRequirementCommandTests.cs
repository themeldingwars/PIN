using AeroMessages.GSS.Character;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.StaticDB.Records.aptfs;
using GameServer.Systems.Aptitude;
using GameServer.Systems.Aptitude.Commands.Requirement;
using GameServer.Tests.Fakes;
using Xunit;

namespace GameServer.Tests;

/// <summary>
///     Requirement commands are evaluated for the entity that owns the chain, and for the proximity abilities of
///     deployables (a glider pad running its launch ability, a thumper, ...) that owner is the deployable and not
///     a character. Answering "this is not a character" with a failure used to remove the effect the ability just
///     applied, after which the client re-triggered the ability, so the effect was applied and lost again dozens
///     of times a second; the boost panel the player was standing on never committed its launch and the status
///     effect churn stalled the client's connection.
///
///     A character only requirement now looks at the character of the activation and only reports "satisfied" when
///     no character is involved at all.
/// </summary>
public class CharacterRequirementCommandTests
{
    [Fact]
    public void RequireCState_DeployableOwner_TestsTheCharacterThatTriggeredIt()
    {
        var shard = new FakeShard();
        var pad = CreateDeployable(shard);
        var player = CreateCharacter(shard, CharacterStateData.CharacterStatus.Living);

        var context = new Context(shard, player) { Self = pad };

        Assert.True(new RequireCStateCommand(CState(living: 1)).Execute(context));
    }

    [Fact]
    public void RequireCState_DeployableOwnerWithDeadPlayer_Fails()
    {
        var shard = new FakeShard();
        var pad = CreateDeployable(shard);
        var player = CreateCharacter(shard, CharacterStateData.CharacterStatus.Dead);

        var context = new Context(shard, player) { Self = pad };

        Assert.False(new RequireCStateCommand(CState(living: 1)).Execute(context));
    }

    [Fact]
    public void RequireCState_WithoutAnyCharacter_Succeeds()
    {
        var shard = new FakeShard();
        var pad = CreateDeployable(shard);
        var other = CreateDeployable(shard);

        var context = new Context(shard, pad);
        context.Targets.Push(other);

        Assert.True(new RequireCStateCommand(CState(living: 1)).Execute(context));
    }

    [Fact]
    public void RequireCState_CharacterOwner_KeepsCheckingThatCharacter()
    {
        var shard = new FakeShard();
        var player = CreateCharacter(shard, CharacterStateData.CharacterStatus.Living);
        var corpse = CreateCharacter(shard, CharacterStateData.CharacterStatus.Dead);

        // FromInitiator names the entity to test, even when it is not a character: the fallback only covers
        // owners that cannot answer the question themselves.
        var context = new Context(shard, corpse) { Self = player };

        Assert.False(new RequireCStateCommand(CState(living: 1, fromInitiator: 1)).Execute(context));
        Assert.True(new RequireCStateCommand(CState(living: 1)).Execute(context));
    }

    [Fact]
    public void RequireCState_PicksACharacterOutOfTheTargetList()
    {
        var shard = new FakeShard();
        var pad = CreateDeployable(shard);
        var player = CreateCharacter(shard, CharacterStateData.CharacterStatus.Living);

        // Both sides of the activation are deployables (a thumper ability running for another one), the player is
        // only in the target list the chain acquired.
        var context = new Context(shard, CreateDeployable(shard)) { Self = pad };
        context.Targets.Push(player);

        Assert.True(new RequireCStateCommand(CState(living: 1)).Execute(context));
    }

    [Fact]
    public void RequireMovestate_DeployableOwner_TestsTheCharacterThatTriggeredIt()
    {
        var shard = new FakeShard();
        var pad = CreateDeployable(shard);
        var player = CreateCharacter(shard, CharacterStateData.CharacterStatus.Living);
        player.MovementStateContainer.MovementStateValue = (ushort)((ushort)Movestate.Glider << 8);

        var context = new Context(shard, player) { Self = pad };

        Assert.True(new RequireMovestateCommand(new RequireMovestateCommandDef { Id = 1568918, Gliding = 1 }).Execute(context));
    }

    [Fact]
    public void RequireMovestate_WithoutAnyCharacter_SucceedsEvenWhenNegated()
    {
        var shard = new FakeShard();
        var pad = CreateDeployable(shard);

        var context = new Context(shard, pad);

        Assert.True(new RequireMovestateCommand(new RequireMovestateCommandDef { Id = 1, Gliding = 1 }).Execute(context));
        Assert.True(new RequireMovestateCommand(new RequireMovestateCommandDef { Id = 2, Gliding = 1, Negate = 1 }).Execute(context));
    }

    [Fact]
    public void RequireLevel_WithoutAnyCharacter_Succeeds()
    {
        var shard = new FakeShard();
        var pad = CreateDeployable(shard);

        var context = new Context(shard, pad);

        Assert.True(new RequireLevelCommand(new RequireLevelCommandDef { Id = 1, FrameLevel = 1, Level = 5 }).Execute(context));
    }

    private static RequireCStateCommandDef CState(byte living = 0, byte fromInitiator = 0)
    {
        return new RequireCStateCommandDef
        {
            Id = 572488,
            Living = living,
            FromInitiator = fromInitiator,
        };
    }

    private static CharacterEntity CreateCharacter(FakeShard shard, CharacterStateData.CharacterStatus state)
    {
        var character = new CharacterEntity(shard, shard.GetNextGuid(0));
        character.SetCharacterState(state, 0);
        shard.Entities.Add(character.EntityId, character);

        return character;
    }

    private static DeployableEntity CreateDeployable(FakeShard shard)
    {
        var entity = new DeployableEntity(shard, shard.GetNextGuid(), type: 395, abilitySrcId: 0);
        shard.Entities.Add(entity.EntityId, entity);

        return entity;
    }
}
