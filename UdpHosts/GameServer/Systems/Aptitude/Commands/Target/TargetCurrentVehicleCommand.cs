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

    public override void Execute(Context context, ref CommandResult result)
    {
        if (context.Self is CharacterEntity { AttachedToEntity: VehicleEntity vehicle })
        {
            context.Targets.Push(vehicle);

            result.SetPass();
            return;
        }

        if (Params.FailNone == 1)
        {
            result.SetFail();
            return;
        }

        result.SetPass();
    }

    public override void Reset(Context context)
    {
        return;
    }
}