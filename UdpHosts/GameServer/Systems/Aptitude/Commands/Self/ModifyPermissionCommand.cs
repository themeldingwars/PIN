using System.Collections.Generic;
using AeroMessages.GSS.Character.Controller;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Self;

public class ModifyPermissionCommand : Command, ICommand
{
    private ModifyPermissionCommandDef Params;

    public ModifyPermissionCommand(ModifyPermissionCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var target = context.Self; // NOTE: Based on glider, it seems like it should use self, maybe that is reasonable for all 'active' style commands?
        if (target is CharacterEntity character)
        {
            context.CommandState[Id] = null;
            foreach (var (flag, value) in Flags())
            {
                character.AddPermissionLayer(flag, context, Id, value);
            }
        }

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
        if (context.CommandState.Remove(Id) && context.Self is CharacterEntity character)
        {
            foreach (var (flag, _) in Flags())
            {
                character.RemovePermissionLayer(flag, context, Id);
            }
        }
    }

    private IEnumerable<(PermissionFlagsData.CharacterPermissionFlags Flag, bool Value)> Flags()
    {
        if (Params.Weapon is { } weapon)
        {
            yield return (PermissionFlagsData.CharacterPermissionFlags.weapon, weapon);
        }

        if (Params.Abilities is { } abilities)
        {
            yield return (PermissionFlagsData.CharacterPermissionFlags.abilities, abilities);
        }

        if (Params.Glider is { } glider)
        {
            yield return (PermissionFlagsData.CharacterPermissionFlags.glider, glider);
        }

        if (Params.GliderHud is { } gliderHud)
        {
            yield return (PermissionFlagsData.CharacterPermissionFlags.glider_hud, gliderHud);
        }

        /*if (Params.Hover is { } hover)
        {
            yield return (PermissionFlagsData.CharacterPermissionFlags.unk, hover);
        }*/

        if (Params.Jetpack is { } jetpack)
        {
            yield return (PermissionFlagsData.CharacterPermissionFlags.jetpack, jetpack);
        }

        if (Params.GenerateSupercharge is { } generateSupercharge)
        {
            yield return (PermissionFlagsData.CharacterPermissionFlags.unk_21, generateSupercharge);
        }
    }
}