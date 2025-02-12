using System;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class RequireLevelCommand : Command, ICommand
{
    private RequireLevelCommandDef Params;

    public RequireLevelCommand(RequireLevelCommandDef par)
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
            if (Params.FrameLevel == 1)
            {
                result = character.Character_BaseController.LevelProp >= Params.Level;
            }
            else if (Params.SessionLevel == 1)
            {
                // todo
                Console.WriteLine($"[RequireLevelCommand] Session level, level {Params.Level}");
                result = true;
            }
        }
        else
        {
            Console.WriteLine("RequireLevelCommand fails because target is not a Character. If this is happening, we should investigate why.");
        }

        return result;
    }
}