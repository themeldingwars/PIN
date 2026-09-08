using System.Linq;
using AeroMessages.GSS.Character;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.StaticDB.Records.aptfs;
using GameServer.Systems.Aptitude;
using GameServer.Systems.Aptitude.Commands.SetFlags;
using GameServer.Tests.Fakes;
using Xunit;

namespace GameServer.Tests;

/// <summary>
///     <c>CombatFlags</c> (aptitude command 64) writes the replicated combat flags of a character for the
///     lifetime of the effect that carries it. The glider pad's launch effect (8097) sets
///     <c>restrict_movement</c> while the 500 ms launch runs, and a weapon scope's status effect (1313) sets
///     <c>restrict_sprint</c> while the player aims. The command is an active: the flags from before the effect
///     applied are restored when it ends, so one effect cannot clear a flag another still-running effect set.
/// </summary>
public class CombatFlagsCommandTests
{
    [Fact]
    public void Apply_SetsTheFlagsTheRowCarries()
    {
        var shard = new FakeShard();
        var character = CreateCharacter(shard);

        var command = new CombatFlagsCommand(new CombatFlagsCommandDef { Id = 1509150, RestrictMovement = 1 });
        var context = RunEffect(command, character);

        Assert.True(character.HasCombatFlag(CombatFlagsData.CharacterCombatFlags.restrict_movement));
    }

    [Fact]
    public void Remove_RestoresTheFlagsFromBefore()
    {
        var shard = new FakeShard();
        var character = CreateCharacter(shard);

        character.SetCombatFlags(new CombatFlagsData
        {
            Value = CombatFlagsData.CharacterCombatFlags.restrict_melee,
            Time = 0,
        });

        var command = new CombatFlagsCommand(new CombatFlagsCommandDef { Id = 1605139, RestrictSprint = 1 });
        var context = RunEffect(command, character);

        Assert.True(character.HasCombatFlag(CombatFlagsData.CharacterCombatFlags.restrict_sprint));

        RemoveEffect(command, context);

        Assert.False(character.HasCombatFlag(CombatFlagsData.CharacterCombatFlags.restrict_sprint));
        Assert.True(character.HasCombatFlag(CombatFlagsData.CharacterCombatFlags.restrict_melee));
    }

    [Fact]
    public void Remove_DoesNotClearFlagsTheRowDidNotTouch()
    {
        var shard = new FakeShard();
        var character = CreateCharacter(shard);

        var command = new CombatFlagsCommand(new CombatFlagsCommandDef { Id = 1509150, RestrictMovement = 1 });
        var context = RunEffect(command, character);

        // Something else sets a different flag while the launch is running.
        character.SetCombatFlags(new CombatFlagsData
        {
            Value = CombatFlagsData.CharacterCombatFlags.restrict_movement | CombatFlagsData.CharacterCombatFlags.knock_down,
            Time = 0,
        });

        RemoveEffect(command, context);

        // The launch only owns restrict_movement; the knockdown another effect set stays.
        Assert.False(character.HasCombatFlag(CombatFlagsData.CharacterCombatFlags.restrict_movement));
        Assert.True(character.HasCombatFlag(CombatFlagsData.CharacterCombatFlags.knock_down));
    }

    [Fact]
    public void DeployableOwner_StepsOverWithoutRegistering()
    {
        var shard = new FakeShard();
        var pad = new DeployableEntity(shard, shard.GetNextGuid(), type: 395, abilitySrcId: 0);
        shard.Entities.Add(pad.EntityId, pad);

        // A pad owned chain can reach this command with the pad as self; combat flags live on a character, so
        // the command must step over a non-character self without failing the chain (and without registering an
        // active that could never apply to anything).
        var command = new CombatFlagsCommand(new CombatFlagsCommandDef { Id = 1509150, RestrictMovement = 1 });
        var context = new Context(shard, pad);

        Assert.True(command.Execute(context));
        Assert.Empty(context.Actives);
    }

    /// <summary>
    ///     Runs what <see cref="AbilitySystem.DoApplyEffect" /> does for one command: the chain (Execute) and then
    ///     the OnApply of everything the command registered as active.
    /// </summary>
    private static Context RunEffect(ICommand command, CharacterEntity character)
    {
        var context = new Context(character.Shard, character);

        Assert.True(command.Execute(context));

        foreach (var pair in context.Actives.Where(pair => pair.Key == command))
        {
            pair.Key.OnApply(context, pair.Value);
        }

        return context;
    }

    private static void RemoveEffect(ICommand command, Context context)
    {
        foreach (var pair in context.Actives.Where(pair => pair.Key == command))
        {
            pair.Key.OnRemove(context, pair.Value);
        }
    }

    private static CharacterEntity CreateCharacter(FakeShard shard)
    {
        var character = new CharacterEntity(shard, shard.GetNextGuid(0));
        character.SetCharacterState(CharacterStateData.CharacterStatus.Living, 0);
        shard.Entities.Add(character.EntityId, character);

        return character;
    }
}
