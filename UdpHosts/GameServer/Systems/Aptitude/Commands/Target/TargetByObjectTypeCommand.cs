using System.Collections.Generic;
using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class TargetByObjectTypeCommand : ICommand
{
    private TargetByObjectTypeCommandDef Params;

    public TargetByObjectTypeCommand(TargetByObjectTypeCommandDef par)
    {
        Params = par;
    }

    // TODO: Handle Params.Projectile
    // TODO: Handle Params.Tinyobject
    // TODO: Handle Params.Vehicle
    public bool Execute(Context context)
    {
        var previousTargets = context.Targets;
        var newTargets = new HashSet<IAptitudeTarget>();
        foreach (IAptitudeTarget target in previousTargets)
        {
            if (Params.Character == 1 && target.GetType() == typeof(Entities.Character.CharacterEntity))
            {
                newTargets.Add(target);
            }
            else if (Params.Deployable == 1 && target.GetType() == typeof(Entities.Deployable.DeployableEntity))
            {
                newTargets.Add(target);
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