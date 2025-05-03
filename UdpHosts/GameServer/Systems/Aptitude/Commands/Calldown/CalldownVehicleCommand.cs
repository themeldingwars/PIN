using System.Numerics;
using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class CalldownVehicleCommand : Command, ICommand
{
    private CalldownVehicleCommandDef Params;

    public CalldownVehicleCommand(CalldownVehicleCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (Params.VehicleId != 0)
        {
            context.Shard.EntityMan.SpawnVehicle(Params.VehicleId, context.InitPosition, Quaternion.Identity, context.Self as CharacterEntity);
        }

        return true;
    }
}