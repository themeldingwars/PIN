using System;
using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireMovestateCommand : ICommand
{
    private RequireMovestateCommandDef Params;

    public RequireMovestateCommand(RequireMovestateCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        bool result = false;

        // NOTE: Investigate target handling
        var target = context.Self;
        
        if (target.GetType() == typeof(Entities.Character.CharacterEntity))
        {
            var character = target as Entities.Character.CharacterEntity;
            var movestate = character.MovementStateContainer.Movestate;

            if (Params.Standing == 1 && (movestate == Entities.Character.Movestate.Standing))
            {
                result = true;
            }
            else if (Params.Running == 1 && (movestate == Entities.Character.Movestate.Running))
            {
                result = true;
            }
            else if (Params.Falling == 1 && (movestate == Entities.Character.Movestate.Falling))
            {
                result = true;
            }
            else if (Params.Sliding == 1 && (movestate == Entities.Character.Movestate.Sliding))
            {
                result = true;
            }
            else if (Params.Walking == 1 && (movestate == Entities.Character.Movestate.Walking))
            {
                result = true;
            }
            else if (Params.Jetpack == 1 && (movestate == Entities.Character.Movestate.Jetpack))
            {
                result = true;
            }
            else if (Params.Gliding == 1 && (movestate == Entities.Character.Movestate.Glider))
            {
                result = true;
            }
            else if (Params.Thruster == 1 && (movestate == Entities.Character.Movestate.GliderThrusters))
            {
                result = true;
            }
            else if (Params.Stall == 1 && (movestate == Entities.Character.Movestate.GliderStalling))
            {
                result = true;
            }
            else if (Params.KnockdownOnground == 1 && (movestate == Entities.Character.Movestate.Knockdown))
            {
                result = true;
            }
            else if (Params.KnockdownFalling == 1 && (movestate == Entities.Character.Movestate.KnockdownFalling))
            {
                result = true;
            }
            else if (Params.JetpackSprint == 1 && (movestate == Entities.Character.Movestate.JetpackSprint))
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