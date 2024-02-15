using System;
using GameServer.Data.SDB.Records.aptfs;
using static AeroMessages.GSS.V66.Character.CharacterStateData;

namespace GameServer.Aptitude;

public class RequireWeaponArmedCommand : ICommand
{
    private RequireWeaponArmedCommandDef Params;

    public RequireWeaponArmedCommand(RequireWeaponArmedCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        bool result = false;

        var target = context.Self;
        if (target.GetType() == typeof(Entities.Character.CharacterEntity))
        {
            var character = target as Entities.Character.CharacterEntity;
            var selectedIndex = character.WeaponIndex.Index;
            
            // The command seems to consider 1 holstered, 2 primary, 3 secondary.
            // The net view uses 0, 1, 2 instead.
            result = Params.WeaponIndex == (selectedIndex + 1);
        }

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}