using System.Linq;
using System.Numerics;
using AeroMessages.GSS.Character;
using AeroMessages.GSS.Character.Controller;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.Enums;
using GameServer.StaticDB.Records.apt;
using GameServer.StaticDB.Records.aptfs;
using GameServer.StaticDB.Records.customdata;
using GameServer.Systems.Aptitude;
using GameServer.Systems.Aptitude.Commands.Duration;
using GameServer.Systems.Aptitude.Commands.Impact;
using GameServer.Systems.Aptitude.Commands.Logic;
using GameServer.Systems.Aptitude.Commands.Modifier;
using GameServer.Systems.Aptitude.Commands.Other;
using GameServer.Systems.Aptitude.Commands.Requirement;
using GameServer.Systems.Aptitude.Commands.Self;
using GameServer.Systems.Aptitude.Commands.SetFlags;
using GameServer.Tests.Fakes;
using Xunit;

namespace GameServer.Tests;

public class EffectLifecycleTests
{
    [Fact]
    public void RuntimeFixture_InitialCharacterViewsCanBeFlushed()
    {
        var (shard, _, character) = CreateRuntime();
        Assert.True(character.Character_ObserverView.GetPackedChangesSize() > 0);
        Assert.True(character.Character_EquipmentView.GetPackedChangesSize() > 0);

        // Do not suppress unrelated view updates to make effect tests pass: the first application flushes
        // all pending character data through the real serializer, even without any scoped-in clients.
        shard.EntityMan.FlushChanges(character);
    }

    [Fact]
    public void Application_SeparatesPredictionTimeAndLifetime()
    {
        var (shard, factory, character) = CreateRuntime(23_224);
        factory.Effects[8097] = MakeEffect(8097, duration: Commands(Timer(500)));

        // The last launch in the report is 67 ms ahead of the server's tick clock.
        Assert.True(shard.Abilities.DoApplyEffect(8097, character, new Context(shard, character) { InitTime = 23_291 }));
        var state = Active(character, 8097);
        Assert.Equal(23_291u, state.Time);
        Assert.Equal(23_291u, character.StatusEffects_0.Value.Time);
        Assert.Equal((ushort)23_291, character.StatusEffectsChangeTime_0);
        Assert.Equal(23_224u, state.Context.EffectStartTime);
        Assert.Equal(23_224ul, state.LastUpdateTime);

        Tick(shard, character, 23_245);
        Assert.False(state.Removed);
        Tick(shard, character, 23_725);
        Assert.True(state.Removed);
    }

    [Theory]
    [InlineData(900u, 1000u, true)] // A future event must not look 49 days old.
    [InlineData(1500u, 1000u, true)]
    [InlineData(1501u, 1000u, false)]
    [InlineData(100u, 4294967040u, true)] // uint wrap, 356 ms elapsed.
    [InlineData(245u, 4294967040u, false)] // uint wrap, 501 ms elapsed.
    public void TimeDuration_UsesSignedModularElapsedTime(uint now, uint start, bool expected)
    {
        var (shard, _, character) = CreateRuntime(now);
        var context = new Context(shard, character) { InitTime = start };
        Assert.Equal(expected, Timer(500).Execute(context));
        Assert.Equal(!expected, new TimeDurationCommand(new TimeDurationCommandDef { DurationMs = 500, Negate = 1 }).Execute(context));
    }

    [Fact]
    public void GliderTransition_GivesSuccessorsTheirOwnLifetimeAndKeepsTheFlightProfile()
    {
        var (shard, factory, character) = CreateRuntime(10_000);
        var pad = new DeployableEntity(shard, shard.GetNextGuid(), type: 395, abilitySrcId: 0);
        character.SetGliderProfileId(7);

        // The relevant prod-1962 graph from the report: 3419 waits 750 ms, then grants 3418.
        // 3418 lasts while airborne OR for its first 500 ms, and applies profile effect 9495.
        // After 2000 ms, 9495 hands off to 3417, which lasts until landing. Client-only visuals omitted.
        factory.Chains[1508823] = Commands(
            new RequireMovestateCommand(new RequireMovestateCommandDef { Id = 1508823, Falling = 1, Gliding = 1, Stall = 1, Thruster = 1 }),
            Timer(500));
        factory.Effects[3419] = MakeEffect(3419, duration: Commands(Timer(750)), remove: Commands(Apply(3418)));
        factory.Effects[3418] = MakeEffect(3418,
            apply: Commands(
                new ModifyPermissionCommand(new ModifyPermissionCommandDef { Id = 1508828, Glider = true }),
                new ModifyPermissionCommand(new ModifyPermissionCommandDef { Id = 1508827, GliderHud = true }),
                Apply(9495)),
            duration: Commands(new LogicOrChainCommand(new LogicOrChainCommandDef { Id = 1508830, OrChain = 1508823 })));
        factory.Effects[9495] = MakeEffect(9495,
            apply: Commands(Profile(1509279, 18)),
            duration: Commands(Timer(2000), new AirborneDurationCommand(new AirborneDurationCommandDef())),
            remove: Commands(Apply(3417)));
        factory.Effects[3417] = MakeEffect(3417,
            apply: Commands(Profile(1511094, 18)),
            duration: Commands(new AirborneDurationCommand(new AirborneDurationCommandDef())));
        factory.Effects[3418].Data.UpdateFrequency = 500;
        factory.Effects[3417].Data.UpdateFrequency = 250;

        var activation = new Context(shard, pad) { InitTime = 10_000, AppliedEffects = [] };
        Assert.True(shard.Abilities.DoApplyEffect(3419, character, activation));
        Tick(shard, character, 10_801);

        var permission = Active(character, 3418);
        Assert.Equal(10_801u, permission.Time);
        Assert.Equal(10_801u, permission.Context.EffectStartTime);
        Assert.Same(pad, permission.Context.Initiator);
        Assert.Same(activation.AppliedEffects, permission.Context.AppliedEffects);
        Assert.True(character.CurrentPermissions[PermissionFlagsData.CharacterPermissionFlags.glider]);
        Assert.Equal(18u, character.GliderProfileId);

        // A standing pose can still be in flight from the client. The NEW permission gets a 500 ms grace
        // period; it must not inherit the 801 ms already spent in 3419 and expire on its first check.
        character.IsAirborne = true;
        Tick(shard, character, 11_002);
        Assert.False(permission.Removed);
        character.MovementStateContainer.MovementStateValue = 0x7000;

        Tick(shard, character, 12_802);
        Assert.False(permission.Removed);
        Assert.DoesNotContain(character.GetActiveEffects(), state => state?.Effect.Id == 9495);
        Assert.Equal(12_802u, Active(character, 3417).Time);
        Assert.Equal(18u, character.GliderProfileId); // Old OnRemove must not overwrite the successor's profile.

        Tick(shard, character, 20_000);
        Assert.Equal(18u, character.GliderProfileId);
        Assert.False(permission.Removed);

        character.IsAirborne = false;
        character.MovementStateContainer.MovementStateValue = 0x1000;
        Tick(shard, character, 20_601);
        Assert.All(character.GetActiveEffects(), Assert.Null);
        Assert.False(character.CurrentPermissions[PermissionFlagsData.CharacterPermissionFlags.glider]);
        Assert.False(character.CurrentPermissions[PermissionFlagsData.CharacterPermissionFlags.glider_hud]);
        Assert.Equal(7u, character.GliderProfileId);
    }

    [Fact]
    public void RemovalChain_PreservesSourceRequirementsButStartsTheChildAtTheHandoff()
    {
        var (shard, factory, character) = CreateRuntime(10_000);
        factory.Effects[1] = MakeEffect(1, duration: Commands(Timer(750)), remove: Commands(
            new TimeDurationCommand(new TimeDurationCommandDef { DurationMs = 500, Negate = 1 }),
            new RequireReloadCommand(new RequireReloadCommandDef { Inittime = 1 }),
            Apply(2)));
        factory.Effects[2] = MakeEffect(2, duration: Commands(Timer(500)));
        Assert.True(shard.Abilities.DoApplyEffect(1, character, new Context(shard, character)));
        var source = Active(character, 1);
        character.Character_CombatView.WeaponReloadedProp = 10_500;

        // The removal chain's requirements still test the old effect's elapsed time/reload history.
        Tick(shard, character, 10_801);
        var child = Active(character, 2);
        Assert.Equal(10_801u, child.Time);
        Assert.Equal(10_801u, child.Context.EffectStartTime);
        Assert.Equal(10_000u, source.Context.InitTime);
        Tick(shard, character, 11_002);
        Assert.False(child.Removed);
        Tick(shard, character, 11_302);
        Assert.True(child.Removed);
    }

    [Fact]
    public void FailedRemovalTail_DoesNotSkipCleanup()
    {
        var (shard, factory, character) = CreateRuntime();
        character.SetGliderProfileId(7);
        factory.Effects[1] = MakeEffect(1, apply: Commands(
            Profile(10, 18),
            new ModifyPermissionCommand(new ModifyPermissionCommandDef { Id = 11, Glider = true }),
            new CombatFlagsCommand(new CombatFlagsCommandDef { Id = 12, RestrictSprint = 1 }),
            new StatModifierCommand(new StatModifierCommandDef { Id = 13, Stat = (ushort)StatModifierIdentifier.RunSpeedMult, Value = 50, Op = 2 })),
            // Like the optional RequireHasItem at the end of the pad's 1508714 removal chain.
            remove: Commands(new ResultCommand(false)));
        Assert.True(shard.Abilities.DoApplyEffect(1, character, new Context(shard, character)));
        var effect = Active(character, 1);
        Assert.Equal(0.5f, character.GetCurrentStatModifierValue(StatModifierIdentifier.RunSpeedMult));
        Assert.False(shard.Abilities.DoRemoveEffect(effect));

        Assert.Equal(7u, character.GliderProfileId);
        Assert.False(character.CurrentPermissions[PermissionFlagsData.CharacterPermissionFlags.glider]);
        Assert.False(character.HasCombatFlag(CombatFlagsData.CharacterCombatFlags.restrict_sprint));
        Assert.Equal(1f, character.GetCurrentStatModifierValue(StatModifierIdentifier.RunSpeedMult));
        Assert.True(shard.Abilities.DoRemoveEffect(effect)); // Already removed; no second cleanup.
    }

    [Fact]
    public void Removal_UnwindsActiveSnapshotsInReverseOrder()
    {
        var (shard, factory, character) = CreateRuntime();
        character.SetGliderProfileId(7);
        factory.Effects[1] = MakeEffect(1, apply: Commands(Profile(1, 18), Profile(2, 25)));
        Assert.True(shard.Abilities.DoApplyEffect(1, character, new Context(shard, character)));
        Assert.Equal(25u, character.GliderProfileId);
        Assert.True(shard.Abilities.DoRemoveEffect(character, 1));
        Assert.Equal(7u, character.GliderProfileId);
    }

    [Fact]
    public void Tick_CascadingRemovalCannotRemoveAReusedSlotAgain()
    {
        var (shard, factory, character) = CreateRuntime();
        factory.Effects[1] = MakeEffect(1, duration: Commands(new ResultCommand(false)), remove: Commands(
            Apply(4),
            new ImpactRemoveEffectCommand(new ImpactRemoveEffectCommandDef { EffectId = 2, RemoveFromSelf = true })));
        factory.Effects[2] = MakeEffect(2, duration: Commands(new ResultCommand(false)), remove: Commands(Apply(3)));
        factory.Effects[3] = MakeEffect(3);
        factory.Effects[4] = MakeEffect(4);
        Assert.True(shard.Abilities.DoApplyEffect(1, character, new Context(shard, character)));
        Assert.True(shard.Abilities.DoApplyEffect(2, character, new Context(shard, character)));
        var oldState = Active(character, 2);

        Tick(shard, character, 61_000);
        var replacement = Active(character, 3);
        Assert.Equal(oldState.Index, replacement.Index);
        Assert.True(oldState.Removed);
        character.ClearEffect(oldState); // A caller still holding the old snapshot must not clear effect 3.
        Assert.Same(replacement, Active(character, 3));
        Assert.False(replacement.Removed);
    }

    [Fact]
    public void UpdateChain_StampsNewEffectsAtTheUpdateWithoutChangingTheParentsStart()
    {
        var (shard, factory, character) = CreateRuntime(10_000);
        factory.Effects[1] = MakeEffect(1, duration: Commands(new ResultCommand(true)));
        factory.Effects[1].UpdateChain = Commands(Apply(2));
        factory.Effects[2] = MakeEffect(2, duration: Commands(Timer(500)));
        Assert.True(shard.Abilities.DoApplyEffect(1, character, new Context(shard, character)));
        Tick(shard, character, 11_000);
        Assert.Equal(11_000u, Active(character, 2).Time);
        Assert.Equal(11_000u, Active(character, 2).Context.EffectStartTime);
        Assert.Equal(10_000u, Active(character, 1).Context.InitTime);
        Assert.Equal(10_000u, Active(character, 1).Context.EffectStartTime);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ImpactApplyEffect_OnlyPassesTheRequestedPayload(bool pass)
    {
        var (shard, factory, character) = CreateRuntime();
        factory.Effects[1] = MakeEffect(1);
        var context = new Context(shard, character)
        {
            Register = 3,
            FormerRegister = 2,
            Bonus = 4,
            InitPosition = Vector3.One,
            AbilityId = 35181,
            AppliedEffects = [],
        };
        var command = new ImpactApplyEffectCommand(new ImpactApplyEffectCommandDef
        {
            EffectId = 1, ApplyToSelf = 1, PassRegister = (byte)(pass ? 1 : 0),
            PassBonus = (byte)(pass ? 1 : 0), InheritInitPos = (byte)(pass ? 1 : 0),
        });
        Assert.True(command.Execute(context));
        var child = Active(character, 1).Context;
        Assert.Equal(pass ? 3f : float.NaN, child.Register);
        Assert.True(float.IsNaN(child.FormerRegister));
        Assert.Equal(pass ? 4 : 0, child.Bonus);
        Assert.Equal(pass ? Vector3.One : character.Position, child.InitPosition);
        Assert.Equal(context.AbilityId, child.AbilityId);
        Assert.Same(context.ActivationInitiator, child.ActivationInitiator);
        Assert.Same(context.PendingCooldowns, child.PendingCooldowns);
        Assert.Same(context.AppliedEffects, child.AppliedEffects);
        Assert.Equal(3f, context.Register);
    }

    private static (FakeShard Shard, FakeAptitudeFactory Factory, CharacterEntity Character) CreateRuntime(ulong time = 60_000)
    {
        var shard = new FakeShard { CurrentTimeLong = time };
        var factory = new FakeAptitudeFactory(shard);
        shard.Abilities = new AbilitySystem(shard, factory);
        var character = FakeCharacterFactory.Create(shard);
        shard.EntityMan.Add(character.EntityId, character);
        return (shard, factory, character);
    }

    private static void Tick(FakeShard shard, CharacterEntity character, ulong time)
    {
        shard.CurrentTimeLong = time;
        shard.Abilities.ProcessTarget(character, time);
    }

    private static EffectState Active(CharacterEntity character, uint id) => Assert.Single(character.GetActiveEffects().Where(state => state?.Effect.Id == id));
    private static Chain Commands(params ICommand[] commands) => new() { Commands = [.. commands] };
    private static TimeDurationCommand Timer(uint ms) => new(new TimeDurationCommandDef { DurationMs = ms });
    private static ImpactApplyEffectCommand Apply(uint id) => new(new ImpactApplyEffectCommandDef { EffectId = id, ApplyToSelf = 1 });
    private static SetGliderParametersCommand Profile(uint id, uint value) => new(new SetGliderParametersCommandDef { Id = id, Value = value });

    private static Effect MakeEffect(uint id, Chain apply = null, Chain duration = null, Chain remove = null) => new()
    {
        Data = new StatusEffectData { Id = id, MaxStackCount = 1, UpdateFrequency = 100 },
        ApplyChain = apply,
        DurationChain = duration,
        RemoveChain = remove,
    };

    private sealed class ResultCommand(bool result) : ICommand
    {
        public uint Id { get; set; }
        public bool Execute(Context context) => result;
    }
}
