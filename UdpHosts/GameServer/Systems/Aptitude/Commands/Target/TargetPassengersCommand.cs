using System;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;
using GameServer.Entities.Vehicle;

namespace GameServer.Aptitude;

public class TargetPassengersCommand : Command, ICommand
{
    private TargetPassengersCommandDef Params;

    public TargetPassengersCommand(TargetPassengersCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (context.Self is not VehicleEntity vehicle)
        {
            Console.WriteLine("TargetPassengersCommand fails because self is not a Vehicle. If this is happening, we should investigate why.");
            return false;
        }

        context.FormerTargets = new AptitudeTargets(context.Targets);

        var targets = new AptitudeTargets();

        if (Params.Filter == 1)
        {
            context.Targets = new AptitudeTargets();

            foreach (var target in context.FormerTargets)
            {
                if (target is not CharacterEntity character)
                {
                    continue;
                }

                if (character.AttachedToEntity != vehicle
                    || vehicle.ControllingPlayer == character.Player)
                {
                    continue;
                }

                targets.Push(target);
            }
        }
        else
        {
            foreach (var occupant in vehicle.Occupants.Values)
            {
                if (occupant.Occupant != vehicle.ControllingPlayer.CharacterEntity)
                {
                    context.Targets.Push((IAptitudeTarget)occupant.Occupant);
                }
            }
        }

        if (Params.FailNone == 1 && context.Targets.Count == 0)
        {
            return false;
        }

        return true;
    }
}