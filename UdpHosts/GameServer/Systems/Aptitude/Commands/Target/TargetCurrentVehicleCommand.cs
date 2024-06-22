using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;
using GameServer.Entities.Vehicle;

namespace GameServer.Aptitude;

public class TargetCurrentVehicleCommand : ICommand
{
    private TargetCurrentVehicleCommandDef Params;

    public TargetCurrentVehicleCommand(TargetCurrentVehicleCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (context.Self is CharacterEntity { AttachedToEntity: VehicleEntity vehicle })
        {
            context.Targets.Push(vehicle);

            return true;
        }

        if (Params.FailNone == 1)
        {
            return false;
        }

        return true;
    }
}
