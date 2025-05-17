using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.Entities.Vehicle;

namespace GameServer.Aptitude;

public class TargetByObjectTypeCommand : Command, ICommand
{
    private TargetByObjectTypeCommandDef Params;

    public TargetByObjectTypeCommand(TargetByObjectTypeCommandDef par)
: base(par)
    {
        Params = par;
    }

    // TODO: Handle Params.Projectile
    // TODO: Handle Params.Tinyobject
    public bool Execute(Context context)
    {
        var previousTargets = context.Targets;
        var newTargets = new AptitudeTargets();
        foreach (IAptitudeTarget target in previousTargets)
        {
            if (Params.Character == 1 && target is CharacterEntity)
            {
                newTargets.Push(target);
            }
            else if (Params.Deployable == 1 && target is DeployableEntity)
            {
                newTargets.Push(target);
            }
            else if (Params.Vehicle == 1 && target is VehicleEntity)
            {
                newTargets.Push(target);
            }
        }

        context.FormerTargets = previousTargets;
        context.Targets = newTargets;

        if (Params.FailNoTargets == 1 && context.Targets.Count == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}