using System;
using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities.Carryable;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.Entities.Vehicle;

namespace GameServer.Aptitude;

public class ApplySinCardCommand : Command, ICommand
{
    private ApplySinCardCommandDef Params;

    public ApplySinCardCommand(ApplySinCardCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // todo aptitude: handle setting SinCardFields
        if (Params.Type == null || Params.Type == 0)
        {
            return true;
        }

        var target = context.Self;

        if (target is CharacterEntity characterEntity)
        {
            characterEntity.Character_ObserverView.SinCardTypeProp = (uint)Params.Type;
        }
        else if (target is DeployableEntity deployableEntity)
        {
            deployableEntity.Deployable_ObserverView.SinCardTypeProp = (uint)Params.Type;
        }
        else if (target is CarryableEntity carryableEntity)
        {
            carryableEntity.CarryableObject_ObserverView.SinCardTypeProp = (uint)Params.Type;
        }
        else if (target is VehicleEntity vehicleEntity)
        {
            vehicleEntity.Vehicle_ObserverView.SinCardTypeProp = (uint)Params.Type;
        }
        else
        {
            Console.WriteLine($"Can't apply SinCard in ApplySinCardCommand {Params.Id}, failing.");
            return false;
        }

        return true;
    }
}