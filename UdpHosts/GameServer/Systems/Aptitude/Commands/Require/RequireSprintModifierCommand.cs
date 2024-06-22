using System;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class RequireSprintModifierCommand : ICommand
{
    private RequireSprintModifierCommandDef Params;

    public RequireSprintModifierCommand(RequireSprintModifierCommandDef par)
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
            result = character.MovementStateContainer.Sprint;
        }
        else
        {
            Console.WriteLine($"RequireSprintModifierCommand fails because target is not a Character. If this is happening, we should investigate why.");
        }

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}