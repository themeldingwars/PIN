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
        var targets = new AptitudeTargets();

        foreach (var target in context.Targets)
        {
            if (target is CharacterEntity character)
            {
                var isPassenger = character.AttachedToEntity is VehicleEntity vehicle
                                  && vehicle.ControllingPlayer != character.Player;

                // todo aptitude: verify Filter param
                if (isPassenger && Params.Filter == 1)
                {
                    continue;
                }

                targets.Push(target);
            }
            else
            {
                Console.WriteLine("TargetPassengersCommand fails because target is not a Character. If this is happening, we should investigate why.");
            }
        }

        context.FormerTargets = context.Targets;
        context.Targets = targets;

        if (Params.FailNone == 1 && targets.Count == 0)
        {
            return false;
        }

        return true;
    }
}