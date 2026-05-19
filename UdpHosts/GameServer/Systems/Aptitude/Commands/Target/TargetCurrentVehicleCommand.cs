using GameServer.Entities.Character;
using GameServer.Entities.Vehicle;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetCurrentVehicleCommand : Command, ICommand
{
    private TargetCurrentVehicleCommandDef Params;

    public TargetCurrentVehicleCommand(TargetCurrentVehicleCommandDef par)
: base(par)
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