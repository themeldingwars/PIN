using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class TargetFilterMovestateCommand : Command, ICommand
{
    private TargetFilterMovestateCommandDef Params;

    public TargetFilterMovestateCommand(TargetFilterMovestateCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        bool result = false;

        var previousTargets = context.Targets;
        var newTargets = new AptitudeTargets();

        foreach (IAptitudeTarget target in previousTargets)
        {
            if (target is not CharacterEntity character)
            {
                continue;
            }

            var movestate = character.MovementStateContainer.Movestate;

            if (Params.Standing == 1 && (movestate == Movestate.Standing))
            {
                newTargets.Push(target);
            }
            else if (Params.Running == 1 && (movestate == Movestate.Running))
            {
                newTargets.Push(target);
            }
            else if (Params.Falling == 1 && (movestate == Movestate.Falling))
            {
                newTargets.Push(target);
            }
            else if (Params.Sliding == 1 && (movestate == Movestate.Sliding))
            {
                newTargets.Push(target);
            }
            else if (Params.Walking == 1 && (movestate == Movestate.Walking))
            {
                newTargets.Push(target);
            }
            else if (Params.Jetpack == 1 && (movestate == Movestate.Jetpack))
            {
                newTargets.Push(target);
            }
            else if (Params.Gliding == 1 && (movestate == Movestate.Glider))
            {
                newTargets.Push(target);
            }
            else if (Params.Thruster == 1 && (movestate == Movestate.GliderThrusters))
            {
                newTargets.Push(target);
            }
            else if (Params.Stall == 1 && (movestate == Movestate.GliderStalling))
            {
                newTargets.Push(target);
            }
            else if (Params.KnockdownOnground == 1 && (movestate == Movestate.Knockdown))
            {
                newTargets.Push(target);
            }
            else if (Params.KnockdownFalling == 1 && (movestate == Movestate.KnockdownFalling))
            {
                newTargets.Push(target);
            }
            else if (Params.JetpackSprint == 1 && (movestate == Movestate.JetpackSprint))
            {
                newTargets.Push(target);
            }
        }

        context.FormerTargets = previousTargets;
        context.Targets = newTargets;

        if (Params.FailNoTargets == 1 && context.Targets.Count == 0)
        {
           result = false;
        }
        else
        {
            result = true;
        }

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}