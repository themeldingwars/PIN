using System.Numerics;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Calldown;

public class CalldownVehicleCommand : Command, ICommand
{
    private CalldownVehicleCommandDef Params;

    public CalldownVehicleCommand(CalldownVehicleCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.VehicleId != 0)
        {
            context.Shard.EntityMan.SpawnVehicle(Params.VehicleId, context.InitPosition, Quaternion.Identity, context.Self as CharacterEntity);
        }

        result.SetPass();
        return;
    }
}