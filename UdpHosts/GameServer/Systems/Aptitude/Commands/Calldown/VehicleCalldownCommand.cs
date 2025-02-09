using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class VehicleCalldownCommand : Command, ICommand
{
    private VehicleCalldownCommandDef Params;

    public VehicleCalldownCommand(VehicleCalldownCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var caller = context.Self;
        var request = context.Abilities.TryConsumeVehicleCalldownRequest(caller.EntityId);
        if (request != null)
        {
            var entityMan = context.Shard.EntityMan;
            var typeId = request.VehicleID;
            var position = request.Position;
            var orientation = request.Rotation;
            entityMan.SpawnVehicle(typeId, position, orientation, caller as CharacterEntity);
            return true;
        }
        else
        {
            return false;
        }
    }
}