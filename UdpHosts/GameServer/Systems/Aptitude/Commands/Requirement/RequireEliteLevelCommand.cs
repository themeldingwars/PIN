using System;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class RequireEliteLevelCommand : Command, ICommand
{
    private RequireEliteLevelCommandDef Params;

    public RequireEliteLevelCommand(RequireEliteLevelCommandDef par)
        : base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        bool result = false;

        // NOTE: Investigate target handling
        var target = context.Self;

        if (target is CharacterEntity character)
        {
            result = character.Character_BaseController.EliteLevelProp >= Params.Level;
        }
        else
        {
            Console.WriteLine("RequireEliteLevelCommand fails because target is not a Character. If this is happening, we should investigate why.");
        }

        return result;
    }
}