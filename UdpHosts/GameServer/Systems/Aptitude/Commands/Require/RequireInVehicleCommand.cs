using System;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;
using GameServer.Entities.Vehicle;

namespace GameServer.Aptitude;

public class RequireInVehicleCommand : ICommand
{
    private RequireInVehicleCommandDef Params;

    public RequireInVehicleCommand(RequireInVehicleCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        bool result = false;

        var target = context.Self;

        if (target is CharacterEntity { AttachedToEntity: VehicleEntity vehicle } character)
        {
            result = (Params.Driver == 1 && vehicle.ControllingPlayer == character.Player)
                     || (Params.Passenger == 1 && vehicle.ControllingPlayer != character.Player);
        }
        else if (target is not CharacterEntity)
        {
            Console.WriteLine("RequireInVehicleCommand fails because target is not a Character. If this is happening, we should investigate why.");
        }

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}