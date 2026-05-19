using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

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