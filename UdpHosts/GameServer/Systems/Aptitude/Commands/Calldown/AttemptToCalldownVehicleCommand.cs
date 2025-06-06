using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class AttemptToCalldownVehicleCommand : Command, ICommand
{
    private AttemptToCalldownVehicleCommandDef Params;

    public AttemptToCalldownVehicleCommand(AttemptToCalldownVehicleCommandDef par)
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

            // Atlernate approach spawning without a request
            /*
            var entityMan = context.Shard.EntityMan;
            var typeId = Params.VehicleType;
            var position = (caller as CharacterEntity).Position;
            var orientation = Quaternion.Identity;
            entityMan.SpawnVehicle(typeId, position, orientation, (Entities.IEntity)caller);
            return true;
            */
        }
    }
}