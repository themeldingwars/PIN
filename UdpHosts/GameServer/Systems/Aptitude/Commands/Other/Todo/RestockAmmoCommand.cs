using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class RestockAmmoCommand : Command, ICommand
{
    private RestockAmmoCommandDef Params;

    public RestockAmmoCommand(RestockAmmoCommandDef par)
: base(par)
    {
        Params = par;
    }

    // todo: should act on targets?
    // abilities: 53, 57
    public bool Execute(Context context)
    {
        if (context.Self is CharacterEntity character)
        {
            // character.Character_CombatController.Ammo_0Prop = max
        }

        return true;
    }
}