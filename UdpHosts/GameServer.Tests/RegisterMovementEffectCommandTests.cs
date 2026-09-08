using System.Linq;
using AeroMessages.GSS.Character;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.StaticDB.Records.aptfs;
using GameServer.Systems.Aptitude;
using GameServer.Systems.Aptitude.Commands.Movement;
using GameServer.Tests.Fakes;
using Xunit;

namespace GameServer.Tests;

/// <summary>
///     <c>RegisterMovementEffect</c> (aptitude command 304) binds a status effect to a movement state for as
///     long as the effect that carries it is active. The same command runs on the client and on the server, and
///     the definition's <c>OnClient</c>/<c>OnServer</c> flags say which machine performs the registration.
///
///     The glider pad's launch chains register the flight effect (723, audio and particles) for the glider and
///     glider-thruster states with <c>on_client=1, on_server=0</c>: that effect is client-side only, so the
///     server must step over the row (a <c>true</c> return, no active) rather than apply it. The rows with
///     <c>on_server=1</c> (the sprint effect 7 for the running state is the canonical one) bind effects the
///     server has to apply itself.
/// </summary>
public class RegisterMovementEffectCommandTests
{
    [Fact]
    public void ClientSideRegistration_ServerStepsOverIt()
    {
        var shard = new FakeShard();
        var character = CreateCharacter(shard);

        // The glider pad's own rows: effect 723 is client audio/particles, the server must not apply it.
        var command = new RegisterMovementEffectCommand(new RegisterMovementEffectCommandDef
        {
            Id = 1508976,
            StatusfxId = 723,
            MovestateIndex = 7,
            OnClient = 1,
            OnServer = 0,
        });

        var context = new Context(shard, character);

        Assert.True(command.Execute(context));
        Assert.Empty(context.Actives);
    }

    [Fact]
    public void ServerSideRegistration_WithADeployableSelf_StepsOver()
    {
        var shard = new FakeShard();
        var pad = new DeployableEntity(shard, shard.GetNextGuid(), type: 395, abilitySrcId: 0);
        shard.Entities.Add(pad.EntityId, pad);

        var command = new RegisterMovementEffectCommand(new RegisterMovementEffectCommandDef
        {
            Id = 997895,
            StatusfxId = 10331,
            MovestateIndex = 10,
            OnServer = 1,
        });

        var context = new Context(shard, pad);

        Assert.True(command.Execute(context));
        Assert.Empty(context.Actives);
    }

    [Fact]
    public void ServerSideRegistration_WithACharacter_RegistersAnActive()
    {
        var shard = new FakeShard();
        var character = CreateCharacter(shard);

        var command = new RegisterMovementEffectCommand(new RegisterMovementEffectCommandDef
        {
            Id = 997895,
            StatusfxId = 10331,
            MovestateIndex = 10,
            OnServer = 1,
        });

        var context = new Context(shard, character);

        Assert.True(command.Execute(context));
        Assert.Single(context.Actives);
    }

    [Fact]
    public void IsInState_ReadsTheMovestateNibble()
    {
        var shard = new FakeShard();
        var character = CreateCharacter(shard);

        var command = new RegisterMovementEffectCommand(new RegisterMovementEffectCommandDef
        {
            Id = 1508976,
            StatusfxId = 723,
            MovestateIndex = 7, // Glider
        });

        // The movestate nibble is (Movestate >> 4): Glider is 0x70, so index 7.
        character.MovementStateContainer.MovementStateValue = (ushort)((ushort)Movestate.Glider << 8);
        Assert.True(command.IsInState(character));

        character.MovementStateContainer.MovementStateValue = (ushort)((ushort)Movestate.Standing << 8);
        Assert.False(command.IsInState(character));
    }

    [Fact]
    public void IsInState_SprintingOnlyCountsWhenTheRowDemandsIt()
    {
        var shard = new FakeShard();
        var character = CreateCharacter(shard);

        // The sprint effect's row: running (index 2) and sprinting.
        var command = new RegisterMovementEffectCommand(new RegisterMovementEffectCommandDef
        {
            Id = 1604741,
            StatusfxId = 7,
            MovestateIndex = 2,
            Sprinting = 1,
            OnServer = 1,
        });

        character.MovementStateContainer.MovementStateValue = (ushort)((ushort)Movestate.Running << 8);
        Assert.False(command.IsInState(character));

        character.MovementStateContainer.Sprint = true;
        Assert.True(command.IsInState(character));
    }

    [Fact]
    public void ApplyAndRemove_WhileOutOfState_RegisterAndUnregisterWithoutTouchingEffects()
    {
        var shard = new FakeShard();
        shard.Abilities = new AbilitySystem(shard);
        var character = CreateCharacter(shard);

        var command = new RegisterMovementEffectCommand(new RegisterMovementEffectCommandDef
        {
            Id = 1604741,
            StatusfxId = 7,
            MovestateIndex = 2,
            Sprinting = 1,
            OnServer = 1,
        });

        var context = new Context(shard, character);

        Assert.True(command.Execute(context));

        // The character is standing, not sprinting: OnApply must register the binding without applying the
        // bound effect, and OnRemove must unregister and take the (never applied) effect off again quietly.
        foreach (var pair in context.Actives.Where(pair => pair.Key == command))
        {
            pair.Key.OnApply(context, pair.Value);
        }

        foreach (var pair in context.Actives.Where(pair => pair.Key == command))
        {
            pair.Key.OnRemove(context, pair.Value);
        }

        Assert.Empty(character.GetActiveEffects().Where(effect => effect != null));
    }

    private static CharacterEntity CreateCharacter(FakeShard shard)
    {
        var character = new CharacterEntity(shard, shard.GetNextGuid(0));
        character.SetCharacterState(CharacterStateData.CharacterStatus.Living, 0);
        shard.Entities.Add(character.EntityId, character);

        return character;
    }
}
