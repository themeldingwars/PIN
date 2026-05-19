using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireMovestateCommand : Command, ICommand
{
    private RequireMovestateCommandDef Params;

    public RequireMovestateCommand(RequireMovestateCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        bool cmdResult = false;

        // NOTE: Investigate target handling
        var target = context.Self;

        if (target is CharacterEntity character)
        {
            var movestate = character.MovementStateContainer.Movestate;

            if (Params.Standing == 1 && (movestate == Movestate.Standing))
            {
                cmdResult = true;
            }
            else if (Params.Running == 1 && (movestate == Movestate.Running))
            {
                cmdResult = true;
            }
            else if (Params.Falling == 1 && (movestate == Movestate.Falling))
            {
                cmdResult = true;
            }
            else if (Params.Sliding == 1 && (movestate == Movestate.Sliding))
            {
                cmdResult = true;
            }
            else if (Params.Walking == 1 && (movestate == Movestate.Walking))
            {
                cmdResult = true;
            }
            else if (Params.Jetpack == 1 && (movestate == Movestate.Jetpack))
            {
                cmdResult = true;
            }
            else if (Params.Gliding == 1 && (movestate == Movestate.Glider))
            {
                cmdResult = true;
            }
            else if (Params.Thruster == 1 && (movestate == Movestate.GliderThrusters))
            {
                cmdResult = true;
            }
            else if (Params.Stall == 1 && (movestate == Movestate.GliderStalling))
            {
                cmdResult = true;
            }
            else if (Params.KnockdownOnground == 1 && (movestate == Movestate.Knockdown))
            {
                cmdResult = true;
            }
            else if (Params.KnockdownFalling == 1 && (movestate == Movestate.KnockdownFalling))
            {
                cmdResult = true;
            }
            else if (Params.JetpackSprint == 1 && (movestate == Movestate.JetpackSprint))
            {
                cmdResult = true;
            }
        }
        else
        {
            Logger.Warning("{Command} {CommandId} fails because target is not a Character. If this is happening, we should investigate why.", nameof(RequireMovestateCommand), Params.Id);
            cmdResult = false;
        }

        if (Params.Negate == 1)
        {
            cmdResult = !cmdResult;
        }

        if (cmdResult)
        {
            result.SetPass();
        }
        else
        {
            result.SetFail();
        }

        return;
    }

    public override void Reset(Context context)
    {
        return;
    }
}