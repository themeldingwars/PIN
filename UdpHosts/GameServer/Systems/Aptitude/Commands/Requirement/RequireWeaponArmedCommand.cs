using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class RequireWeaponArmedCommand : Command, ICommand
{
    private RequireWeaponArmedCommandDef Params;

    public RequireWeaponArmedCommand(RequireWeaponArmedCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        bool result = false;

        var target = context.Self;
        if (target is CharacterEntity character)
        {
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