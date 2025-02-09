using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class RequireNeedsAmmoCommand : Command, ICommand
{
    private RequireNeedsAmmoCommandDef Params;

    public RequireNeedsAmmoCommand(RequireNeedsAmmoCommandDef par)
        : base(par)
    {
        Params = par;
    }

    // todo recheck controller props
    public bool Execute(Context context)
    {
        bool result = false;

        // NOTE: Investigate target handling
        var target = context.Self;

        if (target is CharacterEntity character)
        {
            if (Params.CheckPrimary == 1)
            {
                result = character.Character_CombatController.Ammo_0Prop == 0;
            }

            if (Params.CheckSecondary == 1)
            {
                result = character.Character_CombatController.AltAmmo_0Prop == 0;
            }
        }

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}