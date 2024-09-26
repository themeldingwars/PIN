using System;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class RequireMovingCommand : Command, ICommand
{
    private RequireMovingCommandDef Params;

    public RequireMovingCommand(RequireMovingCommandDef par)
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
            if (Params.CheckVelocity == 1)
            {
                if (Params.Velocitytol == 0)
                {
                    result = !character.MovementStateContainer.Movement;
                }
                else
                {
                    // todo
                    Console.WriteLine($"[RequireMovingCommand] velocity tolerance: {Params.Velocitytol}, negate: {Params.Negate}");
                }
            }
        }
        else
        {
            Console.WriteLine($"RequireMovingCommand fails because target is not a Character. If this is happening, we should investigate why.");
        }

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}