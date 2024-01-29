using System;
using System.Numerics;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyPermissionCommand : ICommand
{
    private ModifyPermissionCommandDef Params;

    public ModifyPermissionCommand(ModifyPermissionCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // TODO: Extend character to support active modifiers that are removed when the source status effect ends        

        var target = context.Self; // NOTE: Based on glider, it seems like it should use self, maybe that is reasonable for all 'active' style commands?
        if (target.GetType() == typeof(Entities.Character.Character))
        {
            var character = target as Entities.Character.Character;

            if (Params.Glider != null)
            {
                context.Actives.Add(Params.Id, new ModifyPermissionActive(this));
                // character.SetPermissionFlag(AeroMessages.GSS.V66.Character.Controller.PermissionFlagsData.CharacterPermissionFlags.glider, (bool)Params.Glider);
            }

            if (Params.GliderHud != null)
            {
                context.Actives.Add(Params.Id, new ModifyPermissionActive(this));
                // character.SetPermissionFlag(AeroMessages.GSS.V66.Character.Controller.PermissionFlagsData.CharacterPermissionFlags.glider_hud, (bool)Params.GliderHud);
            }
        }

        /*
        foreach (IAptitudeTarget target in context.Targets)
        {
            if (target.GetType() == typeof(Entities.Character.Character))
            {
                var character = target as Entities.Character.Character;

                if (Params.Glider != null)
                {
                    character.SetPermissionFlag(AeroMessages.GSS.V66.Character.Controller.PermissionFlagsData.CharacterPermissionFlags.glider, (bool)Params.Glider);
                }

                if (Params.GliderHud != null)
                {
                    character.SetPermissionFlag(AeroMessages.GSS.V66.Character.Controller.PermissionFlagsData.CharacterPermissionFlags.glider_hud, (bool)Params.GliderHud);
                }
            }
        }
        */

        return true;
    }

    public void OnApply(Context context)
    {
        Console.WriteLine($"ModifyPermissionCommand.OnApply Fired");
        var target = context.Self;
        if (target.GetType() == typeof(Entities.Character.Character))
        {
            var character = target as Entities.Character.Character;

            if (Params.Glider != null)
            {
                Console.WriteLine($"ModifyPermissionCommand.OnApply Setting Glider Flag");
                character.SetPermissionFlag(AeroMessages.GSS.V66.Character.Controller.PermissionFlagsData.CharacterPermissionFlags.glider, (bool)Params.Glider);
            }

            if (Params.GliderHud != null)
            {
                character.SetPermissionFlag(AeroMessages.GSS.V66.Character.Controller.PermissionFlagsData.CharacterPermissionFlags.glider_hud, (bool)Params.GliderHud);
            }
        }
    }

    public void OnRemove(Context context)
    {
        Console.WriteLine($"ModifyPermissionCommand.OnRemove Fired");
        var target = context.Self;
        if (target.GetType() == typeof(Entities.Character.Character))
        {
            var character = target as Entities.Character.Character;

            if (Params.Glider != null)
            {
                Console.WriteLine($"ModifyPermissionCommand.OnRemove Removing Glider Flag");
                character.SetPermissionFlag(AeroMessages.GSS.V66.Character.Controller.PermissionFlagsData.CharacterPermissionFlags.glider, (bool)!Params.Glider);
            }

            if (Params.GliderHud != null)
            {
                character.SetPermissionFlag(AeroMessages.GSS.V66.Character.Controller.PermissionFlagsData.CharacterPermissionFlags.glider_hud, (bool)!Params.GliderHud);
            }
        }
    }
}