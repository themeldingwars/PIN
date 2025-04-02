using System;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class RequireMovestateCommand : Command, ICommand
{
    private RequireMovestateCommandDef Params;

    public RequireMovestateCommand(RequireMovestateCommandDef par)
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
            var movestate = character.MovementStateContainer.Movestate;

            if (Params.Standing == 1 && (movestate == Movestate.Standing))
            {
                result = true;
            }
            else if (Params.Running == 1 && (movestate == Movestate.Running))
            {
                result = true;
            }
            else if (Params.Falling == 1 && (movestate == Movestate.Falling))
            {
                result = true;
            }
            else if (Params.Sliding == 1 && (movestate == Movestate.Sliding))
            {
                result = true;
            }
            else if (Params.Walking == 1 && (movestate == Movestate.Walking))
            {
                result = true;
            }
            else if (Params.Jetpack == 1 && (movestate == Movestate.Jetpack))
            {
                result = true;
            }
            else if (Params.Gliding == 1 && (movestate == Movestate.Glider))
            {
                result = true;
            }
            else if (Params.Thruster == 1 && (movestate == Movestate.GliderThrusters))
            {
                result = true;
            }
            else if (Params.Stall == 1 && (movestate == Movestate.GliderStalling))
            {
                result = true;
            }
            else if (Params.KnockdownOnground == 1 && (movestate == Movestate.Knockdown))
            {
                result = true;
            }
            else if (Params.KnockdownFalling == 1 && (movestate == Movestate.KnockdownFalling))
            {
                result = true;
            }
            else if (Params.JetpackSprint == 1 && (movestate == Movestate.JetpackSprint))
            {
                result = true;
            }
        }
        else
        {
            Console.WriteLine($"RequireMovestateCommand fails because target is not a Character. If this is happening, we should investigate why.");
            result = false;
        }

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}