using System.Linq;
using AeroMessages.GSS.Character;
using AeroMessages.GSS.Character.Controller;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.StaticDB.Records.customdata;
using GameServer.Systems.Aptitude;
using GameServer.Systems.Aptitude.Commands.Other;
using GameServer.Systems.Aptitude.Commands.Self;
using GameServer.Tests.Fakes;
using Xunit;

namespace GameServer.Tests;

/// <summary>
///     The commands that grant state a character has to hand back: <c>ModifyPermission</c> (the client side
///     permissions, like gliding) and <c>SetGliderParameters</c> (which glider flight profile the client uses).
///
///     Both write a single value on the character, and both used to have no proper revert: permissions were
///     "restored" by writing the negation of whatever the effect granted, which silently switched a permission off
///     for every other effect that had granted it first, and the glider profile was never restored at all, so a
///     player who used a boost pad kept its flight profile for the rest of the session.
/// </summary>
public class PermissionAndGliderProfileCommandTests
{
    [Fact]
    public void ModifyPermission_RestoresTheValueTheCharacterHadBefore()
    {
        var shard = new FakeShard();
        var character = CreateCharacter(shard);

        // The player's own glider ability granted the permission before the pad's launch effect ran.
        character.SetPermissionFlag(PermissionFlagsData.CharacterPermissionFlags.glider, true);

        var command = new ModifyPermissionCommand(new ModifyPermissionCommandDef { Id = 1508828, Glider = true });
        var context = RunEffect(command, character);

        Assert.True(character.CurrentPermissions[PermissionFlagsData.CharacterPermissionFlags.glider]);

        RemoveEffect(command, context);

        Assert.True(character.CurrentPermissions[PermissionFlagsData.CharacterPermissionFlags.glider]);
    }

    [Fact]
    public void ModifyPermission_TakesTheGrantedPermissionAwayWhenItWasNotSetBefore()
    {
        var shard = new FakeShard();
        var character = CreateCharacter(shard);

        Assert.False(character.CurrentPermissions[PermissionFlagsData.CharacterPermissionFlags.glider]);

        var command = new ModifyPermissionCommand(new ModifyPermissionCommandDef { Id = 1508828, Glider = true });
        var context = RunEffect(command, character);

        Assert.True(character.CurrentPermissions[PermissionFlagsData.CharacterPermissionFlags.glider]);

        RemoveEffect(command, context);

        Assert.False(character.CurrentPermissions[PermissionFlagsData.CharacterPermissionFlags.glider]);
    }

    [Fact]
    public void ModifyPermission_LeavesPermissionsItDidNotChangeAlone()
    {
        var shard = new FakeShard();
        var character = CreateCharacter(shard);

        character.SetPermissionFlag(PermissionFlagsData.CharacterPermissionFlags.jetpack, true);

        var command = new ModifyPermissionCommand(new ModifyPermissionCommandDef { Id = 1508827, GliderHud = true });
        var context = RunEffect(command, character);

        RemoveEffect(command, context);

        Assert.True(character.CurrentPermissions[PermissionFlagsData.CharacterPermissionFlags.jetpack]);
        Assert.False(character.CurrentPermissions[PermissionFlagsData.CharacterPermissionFlags.glider_hud]);
    }

    [Fact]
    public void SetGliderParameters_HandsThePreviousProfileBackWhenTheEffectEnds()
    {
        var shard = new FakeShard();
        var character = CreateCharacter(shard);

        // The glider the player equipped for themselves.
        character.SetGliderProfileId(81423);

        // aptgss::SetGliderParametersDef row of the shared pad launch effect: the pad flies the character
        // with profile 18, the same row the game's own glider effect grants (profile 0 does not exist in
        // the client's dbcharacter::GliderParameters, so it can never be a valid flight model).
        var command = new SetGliderParametersCommand(new SetGliderParametersCommandDef { Id = 1511094, Value = 18 });
        var context = RunEffect(command, character);

        Assert.Equal(18u, character.GliderProfileId);

        RemoveEffect(command, context);

        Assert.Equal(81423u, character.GliderProfileId);
    }

    [Fact]
    public void SetGliderParameters_WithoutDataLeavesTheProfileAlone()
    {
        var shard = new FakeShard();
        var character = CreateCharacter(shard);

        character.SetGliderProfileId(18);

        var command = new SetGliderParametersCommand(new SetGliderParametersCommandDef { Id = 1508824 });
        var context = RunEffect(command, character);

        Assert.Equal(18u, character.GliderProfileId);

        RemoveEffect(command, context);

        Assert.Equal(18u, character.GliderProfileId);
    }

    [Fact]
    public void SetGliderParameters_ZeroIsNotAProfileAndLeavesTheCurrentOneAlone()
    {
        var shard = new FakeShard();
        var character = CreateCharacter(shard);

        character.SetGliderProfileId(18);

        // dbcharacter::GliderParameters has no row 0 (the client table's ids run from 4 up), so a row that
        // carries 0 must not replace the character's flight model with a nonexistent one.
        var command = new SetGliderParametersCommand(new SetGliderParametersCommandDef { Id = 1511094, Value = 0 });
        var context = RunEffect(command, character);

        Assert.Equal(18u, character.GliderProfileId);

        RemoveEffect(command, context);

        Assert.Equal(18u, character.GliderProfileId);
    }

    [Fact]
    public void CommandsOnADeployableOwnerDoNotFailTheChain()
    {
        var shard = new FakeShard();
        var pad = new DeployableEntity(shard, shard.GetNextGuid(), type: 395, abilitySrcId: 0);
        shard.Entities.Add(pad.EntityId, pad);

        var context = new Context(shard, pad);

        Assert.True(new ModifyPermissionCommand(new ModifyPermissionCommandDef { Id = 1508828, Glider = true }).Execute(context));
        Assert.True(new SetGliderParametersCommand(new SetGliderParametersCommandDef { Id = 1511094, Value = 0 }).Execute(context));

        Assert.Empty(context.Actives);
    }

    /// <summary>
    ///     Runs what <see cref="AbilitySystem.DoApplyEffect" /> does for one command: the chain (Execute) and then
    ///     the OnApply of everything the command registered as active.
    /// </summary>
    private static Context RunEffect(ICommand command, CharacterEntity character)
    {
        var shard = character.Shard;
        var context = new Context(shard, character);

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
