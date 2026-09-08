using AeroMessages.GSS.Character;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.StaticDB.Records.aptfs;
using GameServer.Systems.Aptitude;
using GameServer.Systems.Aptitude.Commands.Custom;
using GameServer.Systems.Aptitude.Commands.Duration;
using GameServer.Systems.Aptitude.Commands.Modifier;
using GameServer.Systems.Aptitude.Commands.Requirement;
using GameServer.Systems.Aptitude.Commands.SetFlags;
using GameServer.Tests.Fakes;
using Xunit;

namespace GameServer.Tests;

/// <summary>
///     <c>RequirementServer</c> (aptitude command 110) names the machines whose local simulation may keep
///     executing a chain: <c>Local</c> the machine simulating the chain's entity, <c>LocalInit</c> the machine
///     simulating the initiator, <c>Server</c> the zone server, <c>Client</c> any client. The flags exist for
///     the clients, which run the same chains locally to decide whether the feedback tail after the gate (audio,
///     animation controller, particles) is theirs to run.
///
///     The server used to answer every row without <c>Server=1</c> with a failure, which aborted the apply
///     chain of the effect and cleared the effect again in the same breath it had been replicated with. That is
///     what broke aiming down sights after the scope effect was introduced: the apply chain of a weapon scope's
///     status effect (StatModifier run/jump/thrust penalties, CombatFlags no-sprint) carries a
///     <c>Local=1</c>-only row, so the client received the effect, started the scoped view — and then got the
///     removal, which played out as the weapon blending back to hip fire while the zoom stayed.
/// </summary>
public class RequirementServerCommandTests
{
    [Theory]
    [InlineData(1, 0, 0, 0)]
    [InlineData(0, 1, 0, 0)]
    [InlineData(0, 0, 1, 0)]
    [InlineData(0, 0, 0, 1)]
    [InlineData(1, 0, 0, 1)]
    public void RequirementServer_TheServerQualifiesForEveryMachineGate(int local, int localInit, int server, int client)
    {
        var shard = new FakeShard();
        var character = CreateCharacter(shard);

        var context = new Context(shard, character);

        // The scopes' own row is Local=1 only (1605137 of effect 1313 and its relatives); the others are the
        // remaining machine gates the table carries (initiator's machine, zone server, observers, owner).
        Assert.True(new RequirementServerCommand(Gate((byte)local, (byte)localInit, (byte)server, (byte)client)).Execute(context));
    }

    /// <summary>
    ///     The apply chain of scope effect 1313 (<c>dbitems::WeaponScope.Statusfx</c> of scopes 10018/30691):
    ///     the stat penalties and the no-sprint flag a character carries while aiming down sights, with the
    ///     client feedback commands gated behind the <c>Local</c>-only RequirementServer row. The chain has to
    ///     succeed as a whole — a false return is what <see cref="AbilitySystem.DoApplyEffect(uint, IAptitudeTarget, Context)" />
    ///     answers by clearing the effect again — and the stat modifier has to stay registered so its OnApply runs.
    /// </summary>
    [Fact]
    public void RequirementServer_DoesNotAbortTheScopeEffectApplyChain()
    {
        var shard = new FakeShard();
        var character = CreateCharacter(shard);

        // apt::BaseCommandDef 1605142 -> 1605136 of effect 1313, client commands as the no-ops they are here.
        var applyChain = new Chain
        {
            Id = 1605142,
            Commands =
            [
                new StatModifierCommand(new StatModifierCommandDef { Id = 1605142, Stat = (ushort)StatModifierIdentifier.RunSpeedMult, Value = 50, Op = 2 }),
                new StatModifierCommand(new StatModifierCommandDef { Id = 1605141, Stat = (ushort)StatModifierIdentifier.ThrustStrengthMult, Value = 50, Op = 2 }),
                new StatModifierCommand(new StatModifierCommandDef { Id = 1605140, Stat = (ushort)StatModifierIdentifier.JumpHeightMult, Value = 60, Op = 2 }),
                new CombatFlagsCommand(new CombatFlagsCommandDef { Id = 1605139, RestrictSprint = 1 }),
                new CustomNOOPCommand("tfSetAnimCtrlParamCommandDef", 1605138),
                new RequirementServerCommand(Gate(local: 1, localInit: 0, server: 0, client: 0)),
                new CustomNOOPCommand("tfAudioFeedbackCommandDef", 1605136)
            ]
        };

        var context = new Context(shard, character);
        Assert.True(applyChain.Execute(context));

        // What AbilitySystem.DoApplyEffect does once the chain succeeded: the registered actives apply.
        foreach (var pair in context.Actives)
        {
            pair.Key.OnApply(context, pair.Value);
        }

        Assert.Equal(0.5f, character.GetCurrentStatModifierValue(StatModifierIdentifier.RunSpeedMult));
        Assert.Equal(0.5f, character.GetCurrentStatModifierValue(StatModifierIdentifier.ThrustStrengthMult));
        Assert.Equal(0.6f, character.GetCurrentStatModifierValue(StatModifierIdentifier.JumpHeightMult));

        // And what its removal hands back when the player scopes out again.
        foreach (var pair in context.Actives)
        {
            pair.Key.OnRemove(context, pair.Value);
        }

        Assert.Equal(1.0f, character.GetCurrentStatModifierValue(StatModifierIdentifier.RunSpeedMult));
    }

    /// <summary>
    ///     The duration chain of the same effect: the scoped state has to survive the ability system's tick
    ///     that runs it, so nothing but scoping out (or the character no longer living) takes it away.
    /// </summary>
    [Fact]
    public void ScopeEffectDurationChain_KeepsTheEffectAliveWhileTheCharacterIsLiving()
    {
        var shard = new FakeShard();
        var character = CreateCharacter(shard);

        // apt::BaseCommandDef 1605146 -> 1605144 of effect 1313.
        var durationChain = new Chain
        {
            Id = 1605146,
            Commands =
            [
                new BattleFrameDurationCommand(new BattleFrameDurationCommandDef { Id = 1605146, Notchanged = 1 }),
                new RequireCStateCommand(new RequireCStateCommandDef { Id = 1605145, Living = 1 }),
                new CustomNOOPCommand("tfRequireServerConfirmedCommandDef", 1605144)
            ]
        };

        var context = new Context(shard, character);

        Assert.True(durationChain.Execute(context));

        character.SetCharacterState(CharacterStateData.CharacterStatus.Dead, 0);

        Assert.False(durationChain.Execute(context));
    }

    private static RequirementServerCommandDef Gate(byte local, byte localInit, byte server, byte client)
    {
        return new RequirementServerCommandDef
        {
            Id = 1605137,
            Local = local,
            LocalInit = localInit,
            Server = server,
            Client = client,
        };
    }

    private static CharacterEntity CreateCharacter(FakeShard shard)
    {
        var character = new CharacterEntity(shard, shard.GetNextGuid(0));
        character.SetCharacterState(CharacterStateData.CharacterStatus.Living, 0);
        shard.Entities.Add(character.EntityId, character);

        return character;
    }
}
