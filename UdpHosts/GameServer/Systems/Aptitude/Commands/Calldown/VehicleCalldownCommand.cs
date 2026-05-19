using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Calldown;

public class VehicleCalldownCommand : Command, ICommand
{
    private VehicleCalldownCommandDef Params;

    public VehicleCalldownCommand(VehicleCalldownCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
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
            result.SetPass();
            return;
        }
        else
        {
            result.SetFail();
            return;
        }
    }
}