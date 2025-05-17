using AeroMessages.GSS.V66.Character.Controller;
using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class ModifyPermissionCommand : Command, ICommand
{
    private ModifyPermissionCommandDef Params;

    public ModifyPermissionCommand(ModifyPermissionCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var target = context.Self; // NOTE: Based on glider, it seems like it should use self, maybe that is reasonable for all 'active' style commands?
        if (target is CharacterEntity)
        {
            if (Params.Glider != null)
            {
                context.Actives.Add(this, null);
            }

            if (Params.GliderHud != null)
            {
                context.Actives.Add(this, null);
            }

            /*if (Params.Hover != null)
            {
                context.Actives.Add(this, null);
            }*/

            if (Params.Jetpack != null)
            {
                context.Actives.Add(this, null);
            }
        }

        return true;
    }

    public void OnApply(Context context, ICommandActiveContext activeCommandContext)
    {
        var target = context.Self;
        if (target is CharacterEntity character)
        {
            if (Params.Glider != null)
            {
                character.SetPermissionFlag(PermissionFlagsData.CharacterPermissionFlags.glider, (bool)Params.Glider);
            }

            if (Params.GliderHud != null)
            {
                character.SetPermissionFlag(PermissionFlagsData.CharacterPermissionFlags.glider_hud, (bool)Params.GliderHud);
            }

            /*if (Params.Hover != null)
            {
                character.SetPermissionFlag(PermissionFlagsData.CharacterPermissionFlags.unk, (bool)Params.Hover);
            }*/

            if (Params.Jetpack != null)
            {
                character.SetPermissionFlag(PermissionFlagsData.CharacterPermissionFlags.jetpack, (bool)Params.Jetpack);
            }
        }
    }

    public void OnRemove(Context context, ICommandActiveContext activeCommandContext)
    {
        var target = context.Self;
        if (target is CharacterEntity character)
        {
            if (Params.Glider != null)
            {
                character.SetPermissionFlag(PermissionFlagsData.CharacterPermissionFlags.glider, (bool)!Params.Glider);
            }

            if (Params.GliderHud != null)
            {
                character.SetPermissionFlag(PermissionFlagsData.CharacterPermissionFlags.glider_hud, (bool)!Params.GliderHud);
            }

            /*if (Params.Hover != null)
            {
                character.SetPermissionFlag(PermissionFlagsData.CharacterPermissionFlags.unk, (bool)!Params.Hover);
            }*/

            if (Params.Jetpack != null)
            {
                character.SetPermissionFlag(PermissionFlagsData.CharacterPermissionFlags.jetpack, (bool)!Params.Jetpack);
            }
        }
    }
}