using System;
using System.Buffers.Binary;
using System.Linq;
using System.Reflection;
using AeroMessages.GSS.Character;
using AeroMessages.GSS.Character.Controller;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.Packets;
using GameServer.StaticDB.Records.apt;
using GameServer.StaticDB.Records.aptfs;
using GameServer.Systems.Aptitude;
using GameServer.Systems.Aptitude.Commands.Custom;
using GameServer.Systems.Aptitude.Commands.Modifier;
using GameServer.Systems.Aptitude.Commands.Requirement;
using GameServer.Systems.Aptitude.Commands.SetFlags;
using GameServer.Tests.Fakes;
using Xunit;
using ScopeController = GameServer.Controllers.Character.CombatController;

namespace GameServer.Tests;

public class ScopedStateTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    public void UseScope_HoldsOneEffectAndKeepsTheClientEventTime(int inScope)
    {
        var (shard, character, player, controller) = CreateRuntime();
        controller.UseScope(player, player, character.EntityId, Packet(60_001, unchecked((byte)inScope)));
        var state = Assert.Single(character.GetActiveEffects().Where(effect => effect != null));
        Assert.Equal(1313u, state.Effect.Id);
        Assert.Equal(60_001u, state.Time);
        Assert.Equal(60_000u, state.Context.EffectStartTime);
        Assert.Equal((byte)1, character.FireMode_1.Mode);
        Assert.Equal((byte)0, character.GetActiveFireModeIndex()); // ADS must not select the underbarrel.
        Assert.True(character.HasCombatFlag(CombatFlagsData.CharacterCombatFlags.restrict_sprint));
        Assert.Equal(0.5f, character.GetCurrentStatModifierValue(StatModifierIdentifier.RunSpeedMult));

        shard.CurrentTimeLong = 62_000;
        shard.Abilities.ProcessTarget(character, 62_000);
        controller.UseScope(player, player, character.EntityId, Packet(62_000, 1));
        Assert.Same(state, Assert.Single(character.GetActiveEffects().Where(effect => effect != null)));
        Assert.Equal(60_001u, state.Time);
        Assert.Equal(60_001u, character.Character_CombatController.StatusEffects_0Prop.Value.Time);
        Assert.Equal(60_001u, character.Character_LocalEffectsController.LocalStatusEffects_0Prop.Value.Time);

        controller.UseScope(player, player, character.EntityId, Packet(62_001, 0));
        AssertUnscoped(character);
        Assert.True(state.Removed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WeaponOrFireModeSwitch_ClearsTheScopedModeAsWellAsTheEffect(bool fireMode)
    {
        var (_, character, player, controller) = CreateRuntime();
        controller.UseScope(player, player, character.EntityId, Packet(60_001, 1));
        if (fireMode)
        {
            controller.SelectFireMode(player, player, character.EntityId, Packet(60_002, 1));
            Assert.Equal((byte)1, character.GetActiveFireModeIndex());
        }
        else
        {
            controller.SelectWeapon(player, player, character.EntityId, Packet(60_002, 2, 0));
            Assert.Equal((byte)2, character.WeaponIndex.Index);
        }

        AssertUnscoped(character);
        Assert.Equal(60_002u, character.FireMode_1.Time);
        Assert.Equal(character.FireMode_1, character.Character_CombatController.FireMode_1Prop);
        Assert.Equal(character.FireMode_1, character.Character_CombatView.FireMode_1Prop);

        controller.UseScope(player, player, character.EntityId, Packet(60_003, 1));
        Assert.Equal(102u, Assert.Single(character.GetActiveEffects().Where(effect => effect != null)).Effect.Id);
    }

    [Fact]
    public void ExternalEffectRemoval_ClearsTheModeAndAllowsTheNextScopeRequest()
    {
        var (shard, character, player, controller) = CreateRuntime();
        controller.UseScope(player, player, character.EntityId, Packet(60_001, 1));
        shard.CurrentTimeLong = 61_000;
        Assert.True(shard.Abilities.DoRemoveEffect(character, 1313));
        AssertUnscoped(character);

        controller.UseScope(player, player, character.EntityId, Packet(61_001, 1));
        Assert.Equal(1313u, character.ScopeStatusEffectId);
        Assert.Single(character.GetActiveEffects().Where(effect => effect != null));
    }

    [Fact]
    public void Death_ClearsBothHalvesImmediatelyAndRejectsScopingWhileDead()
    {
        var (_, character, player, controller) = CreateRuntime();
        controller.UseScope(player, player, character.EntityId, Packet(60_001, 1));
        character.SetCharacterState(CharacterStateData.CharacterStatus.Dead, 60_002);
        AssertUnscoped(character);
        controller.UseScope(player, player, character.EntityId, Packet(60_003, 1));
        AssertUnscoped(character);
    }

    [Fact]
    public void StaleScopeOut_DoesNotCancelANewerScopeIn()
    {
        var (_, character, player, controller) = CreateRuntime();
        controller.UseScope(player, player, character.EntityId, Packet(60_200, 1));
        controller.UseScope(player, player, character.EntityId, Packet(60_100, 0));
        Assert.Equal((byte)1, character.FireMode_1.Mode);
        Assert.Equal(60_200u, character.FireMode_1.Time);
        Assert.Equal(1313u, character.ScopeStatusEffectId);
    }

    [Fact]
    public void ClockWrap_ZeroRemainsAValidScopePredictionTimestamp()
    {
        var (_, character, player, controller) = CreateRuntime(uint.MaxValue - 1ul);
        controller.UseScope(player, player, character.EntityId, Packet(0, 1));
        var state = Assert.Single(character.GetActiveEffects().Where(effect => effect != null));
        Assert.Equal(0u, state.Time);
        Assert.Equal(0u, character.FireMode_1.Time);
        controller.UseScope(player, player, character.EntityId, Packet(uint.MaxValue, 0));
        Assert.Equal((byte)1, character.FireMode_1.Mode); // Before the wrap: stale.
        controller.UseScope(player, player, character.EntityId, Packet(1, 0));
        AssertUnscoped(character);
    }

    [Fact]
    public void UnknownScopeEffect_DoesNotLeaveTheScopedModeSet()
    {
        var (shard, character, player, controller) = CreateRuntime();
        ((FakeAptitudeFactory)shard.Abilities.Factory).Effects.Clear();
        controller.UseScope(player, player, character.EntityId, Packet(60_001, 1));
        AssertUnscoped(character);
    }

    private static void AssertUnscoped(CharacterEntity character)
    {
        Assert.Equal((byte)0, character.FireMode_1.Mode);
        Assert.Equal(0u, character.ScopeStatusEffectId);
        Assert.All(character.GetActiveEffects(), Assert.Null);
        Assert.Null(character.Character_CombatController.StatusEffects_0Prop);
        Assert.Null(character.Character_LocalEffectsController.LocalStatusEffects_0Prop);
        Assert.False(character.HasCombatFlag(CombatFlagsData.CharacterCombatFlags.restrict_sprint));
        Assert.Equal(1f, character.GetCurrentStatModifierValue(StatModifierIdentifier.RunSpeedMult));
    }

    private static (FakeShard Shard, CharacterEntity Character, FakeNetworkPlayer Player, ScopeController Controller) CreateRuntime(ulong time = 60_000)
    {
        var shard = new FakeShard { CurrentTimeLong = time };
        var factory = new FakeAptitudeFactory(shard);
        shard.Abilities = new AbilitySystem(shard, factory);
        factory.Effects[1313] = ScopeEffect(1313);
        factory.Effects[102] = ScopeEffect(102);
        var character = FakeCharacterFactory.Create(shard);
        // Keep the replicated controller fields observable without opening any network channels.
        character.Character_CombatController = new AeroMessages.GSS.Character.Controller.CombatController();
        character.Character_LocalEffectsController = new LocalEffectsController();
        shard.EntityMan.Add(character.EntityId, character);
        character.SetWeaponIndex(new WeaponIndexData { Index = 1, Time = unchecked((uint)time) });

        // Only bypass the SDB-backed weapon-cache builder; requests, effect execution, expiry, cleanup and
        // replicated field setters all run through production code.
        var weapons = new CharacterEntity.ActiveWeaponDetails[3, 2];
        weapons[1, 0] = new CharacterEntity.ActiveWeaponDetails { ScopeStatusFx = 1313 };
        weapons[1, 1] = new CharacterEntity.ActiveWeaponDetails { ScopeStatusFx = 102 };
        weapons[2, 0] = new CharacterEntity.ActiveWeaponDetails { ScopeStatusFx = 102 };
        typeof(CharacterEntity).GetField("_weaponDetailsCache", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(character, weapons);

        var player = new FakeNetworkPlayer(shard) { CharacterEntity = character };
        var controller = new ScopeController();
        controller.Init(player, player, shard, shard.Logger);
        return (shard, character, player, controller);
    }

    private static Effect ScopeEffect(uint id) => new()
    {
        Data = new StatusEffectData { Id = id, MaxStackCount = 1, UpdateFrequency = 250 },
        ApplyChain = new Chain
        {
            Commands =
            [
                new StatModifierCommand(new StatModifierCommandDef { Id = 1605142, Stat = (ushort)StatModifierIdentifier.RunSpeedMult, Value = 50, Op = 2 }),
                new CombatFlagsCommand(new CombatFlagsCommandDef { Id = 1605139, RestrictSprint = 1 }),
                new RequirementServerCommand(new RequirementServerCommandDef { Id = 1605137, Local = 1 }),
            ],
        },
        DurationChain = new Chain
        {
            Commands =
            [
                new RequireCStateCommand(new RequireCStateCommandDef { Id = 1605145, Living = 1 }),
                new CustomNOOPCommand("RequireServerConfirmed", 1605144),
            ],
        },
    };

    private static GamePacket Packet(uint time, params byte[] fields)
    {
        var data = new byte[sizeof(uint) + fields.Length];
        BinaryPrimitives.WriteUInt32LittleEndian(data, time);
        fields.CopyTo(data, sizeof(uint));
        return new GamePacket(default, data);
    }
}
